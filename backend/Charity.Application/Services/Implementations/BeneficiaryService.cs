using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class BeneficiaryService : IBeneficiaryService
{
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly IRepository<BeneficiaryFamilyMember> _familyMemberRepo;
    private readonly IRepository<BeneficiaryHeadHistory> _headHistoryRepo;
    private readonly IRepository<Distribution> _distributionRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public BeneficiaryService(
        IRepository<Beneficiary> beneficiaryRepo,
        IRepository<BeneficiaryFamilyMember> familyMemberRepo,
        IRepository<BeneficiaryHeadHistory> headHistoryRepo,
        IRepository<Distribution> distributionRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _beneficiaryRepo = beneficiaryRepo;
        _familyMemberRepo = familyMemberRepo;
        _headHistoryRepo = headHistoryRepo;
        _distributionRepo = distributionRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<BeneficiaryDto>> GetAllAsync()
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var entities = await _beneficiaryRepo.GetAllAsync();
        var allMembers = await _familyMemberRepo.GetAllAsync();
        var membersGrouped = allMembers.GroupBy(m => m.BeneficiaryId).ToDictionary(g => g.Key, g => g.ToList());
        var allHistories = await _headHistoryRepo.GetAllAsync();
        var historiesGrouped = allHistories.GroupBy(h => h.BeneficiaryId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var b in entities)
        {
            if (membersGrouped.TryGetValue(b.Id, out var fList))
            {
                b.FamilyMembers = fList;
            }
            if (historiesGrouped.TryGetValue(b.Id, out var hList))
            {
                b.HeadHistories = hList;
            }
        }

        return entities.Select(b => b.ToDto(currentCharityId)).ToList();
    }

    public async Task<BeneficiaryDto?> GetByIdAsync(string id)
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var entity = await _beneficiaryRepo.GetByIdAsync(id);
        if (entity == null) return null;

        var members = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == id);
        entity.FamilyMembers = members.ToList();

        var histories = await _headHistoryRepo.FindAsync(h => h.BeneficiaryId == id);
        entity.HeadHistories = histories.ToList();

        return entity.ToDto(currentCharityId);
    }

    public async Task<BeneficiaryDto> CreateAsync(CreateBeneficiaryDto dto)
    {
        var charityId = _currentUserService.CharityId ?? "c1";
        var charityName = _currentUserService.CharityName ?? "الجمعية المسجلة";

        // التدقيق في عدم تكرار الرقم القومي سواء لصاحب الملف أو لأفراد الأسرة
        await ValidateNationalIdsAsync(null, dto.NationalId, dto.FamilyMembers);

        var entity = dto.ToEntity(charityId, charityName);
        await _beneficiaryRepo.AddAsync(entity);

        if (dto.FamilyMembers != null && dto.FamilyMembers.Count > 0)
        {
            foreach (var fmDto in dto.FamilyMembers)
            {
                var fmEntity = fmDto.ToEntity(entity.Id);
                entity.FamilyMembers.Add(fmEntity);
            }
            entity.FamilyCount = Math.Max(entity.FamilyCount, dto.FamilyMembers.Count + 1);
        }

        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto(charityId);
    }

    public async Task<BeneficiaryDto> UpdateAsync(string id, UpdateBeneficiaryDto dto)
    {
        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;

        // Strict Authorization Rule: ONLY the registering charity can update beneficiary CRM profile!
        if (!string.IsNullOrEmpty(existing.RegisteredByCharityId) && existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: لا تملك صلاحية تعديل بيانات هذا المستفيد. التعديل مقصور قانونياً على الجمعية التي قامت بفتح الملف ({existing.RegisteredByCharityName})."
            );
        }

        // التدقيق في عدم تكرار الرقم القومي
        await ValidateNationalIdsAsync(id, dto.NationalId, dto.FamilyMembers);

        existing.NationalId = dto.NationalId.Trim();
        existing.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.FamilyCode)) existing.FamilyCode = dto.FamilyCode.Trim();
        if (!string.IsNullOrWhiteSpace(dto.FamilyName)) existing.FamilyName = dto.FamilyName.Trim();
        existing.Phone = dto.Phone;
        existing.Category = dto.Category;
        existing.MaritalStatus = dto.MaritalStatus;
        existing.FamilyCount = dto.FamilyCount;
        existing.Address = dto.Address;
        existing.Housing = dto.Housing;
        existing.HealthIssues = dto.HealthIssues;
        existing.ResearchNotes = dto.ResearchNotes;
        existing.Status = dto.Status;

        // البيانات الجغرافية ودرجة الاستحقاق
        existing.VillageOrDistrict = dto.VillageOrDistrict ?? string.Empty;
        existing.PriorityTier = dto.PriorityTier ?? "أ - أولوية قصوى ومعدم";

        // مواصفات السكن والأسقف والمرافق
        existing.RoofType = dto.RoofType ?? "خرساني مسلح";
        existing.WallType = dto.WallType ?? "طوب أحمر";
        existing.WaterSource = dto.WaterSource ?? "عداد مياه رسمي";
        existing.SanitationType = dto.SanitationType ?? "شبكة صرف عمومية";
        existing.ElectricitySource = dto.ElectricitySource ?? "عداد رسمي";
        existing.AppliancesSummary = dto.AppliancesSummary ?? string.Empty;

        // ملف الديون وإيصالات الأمانة وخطر الحبس (الغارمين)
        existing.HasDebts = dto.HasDebts;
        existing.TotalDebtAmount = dto.TotalDebtAmount;
        existing.DebtCreditor = dto.DebtCreditor ?? string.Empty;
        existing.IsThreatenedWithImprisonment = dto.IsThreatenedWithImprisonment;
        existing.DebtNotes = dto.DebtNotes ?? string.Empty;

        // التمكين الاقتصادي والمشروعات متناهية الصغر
        existing.IsCandidateForMicroProject = dto.IsCandidateForMicroProject;
        existing.SuggestedMicroProject = dto.SuggestedMicroProject ?? string.Empty;

        // البيانات المالية والصحية الجديدة
        existing.IncomeSource = dto.IncomeSource ?? string.Empty;
        existing.MonthlyIncome = dto.MonthlyIncome;
        existing.HasLandOrProperty = dto.HasLandOrProperty;
        existing.LandOrPropertyDetails = dto.LandOrPropertyDetails ?? string.Empty;
        existing.HasGovernmentPension = dto.HasGovernmentPension;
        existing.GovernmentPensionDetails = dto.GovernmentPensionDetails ?? string.Empty;
        existing.HasChronicDisease = dto.HasChronicDisease;
        existing.ChronicDiseaseDetails = dto.ChronicDiseaseDetails ?? string.Empty;
        existing.NeedsPeriodicMedication = dto.NeedsPeriodicMedication;
        existing.PeriodicMedicationDetails = dto.PeriodicMedicationDetails ?? string.Empty;
        existing.MedicationFrequency = dto.MedicationFrequency ?? string.Empty;

        // مزامنة أفراد الأسرة
        var existingMembers = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == id);
        var incomingIds = dto.FamilyMembers?.Where(m => !string.IsNullOrWhiteSpace(m.Id)).Select(m => m.Id).ToHashSet() ?? [];
        foreach (var oldMember in existingMembers)
        {
            if (!incomingIds.Contains(oldMember.Id)) await _familyMemberRepo.DeleteAsync(oldMember);
        }

        existing.FamilyMembers = new List<BeneficiaryFamilyMember>();
        if (dto.FamilyMembers != null && dto.FamilyMembers.Count > 0)
        {
            foreach (var fmDto in dto.FamilyMembers)
            {
                var fmEntity = fmDto.ToEntity(existing.Id);
                if (existingMembers.Any(m => m.Id == fmEntity.Id))
                    await _familyMemberRepo.UpdateAsync(fmEntity);
                else
                    await _familyMemberRepo.AddAsync(fmEntity);
            }
            existing.FamilyCount = Math.Max(existing.FamilyCount, dto.FamilyMembers.Count + 1);
        }

        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        existing.FamilyMembers = (await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == id)).ToList();
        return existing.ToDto(currentCharityId);
    }

    public async Task<BeneficiaryAidSummaryDto> GetAidSummaryAsync(string beneficiaryId)
    {
        var beneficiary = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;

        // Fetch all distributions from ALL charities for this beneficiary
        var allDistributions = await _distributionRepo.FindAsync(d => d.BeneficiaryId == beneficiaryId);

        var aidHistoryDtos = allDistributions
            .OrderByDescending(d => d.Date)
            .Select(d => d.ToDto(currentCharityId))
            .ToList();

        var otherCharitiesCount = aidHistoryDtos.Count(d => !d.IsFromCurrentCharity);

        return new BeneficiaryAidSummaryDto
        {
            BeneficiaryId = beneficiary.Id,
            BeneficiaryName = beneficiary.Name,
            TotalDistributionsCount = aidHistoryDtos.Count,
            OtherCharitiesDistributionsCount = otherCharitiesCount,
            AidHistory = aidHistoryDtos
        };
    }

    public async Task<BeneficiaryDto> SuspendAsync(string id, SuspendBeneficiaryDto dto)
    {
        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;

        if (!string.IsNullOrEmpty(existing.RegisteredByCharityId) && existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: إيقاف الملف مقصور على الجمعية المسجلة له ({existing.RegisteredByCharityName})."
            );
        }

        existing.IsSuspended = true;
        existing.Status = "موقوف";
        existing.SuspensionReasonId = dto.SuspensionReasonId;
        existing.SuspensionReasonTitle = string.IsNullOrWhiteSpace(dto.SuspensionReasonTitle) ? "إيقاف إداري" : dto.SuspensionReasonTitle.Trim();
        existing.SuspensionNotes = dto.Notes;
        existing.SuspendedAt = DateTime.UtcNow;
        existing.SuspendedByCharityId = currentCharityId;
        existing.SuspendedByCharityName = _currentUserService.CharityName ?? "الجمعية المسجلة";

        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var members = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == id);
        existing.FamilyMembers = members.ToList();

        return existing.ToDto(currentCharityId);
    }

    public async Task<BeneficiaryDto> ReactivateAsync(string id)
    {
        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;

        if (!string.IsNullOrEmpty(existing.RegisteredByCharityId) && existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: إعادة تفعيل الملف مقصورة على الجمعية المسجلة له ({existing.RegisteredByCharityName})."
            );
        }

        existing.IsSuspended = false;
        existing.Status = "مستحق فعال";
        existing.SuspensionNotes = (existing.SuspensionNotes ?? "") + $" [تم إعادة التفعيل بتاريخ {DateTime.Now:yyyy-MM-dd}]";

        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var members = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == id);
        existing.FamilyMembers = members.ToList();

        return existing.ToDto(currentCharityId);
    }

    public async Task<BeneficiaryDto> SeparateFamilyMemberAsync(string beneficiaryId, SeparateFamilyMemberDto dto)
    {
        var existing = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;

        if (!string.IsNullOrEmpty(existing.RegisteredByCharityId) && existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: إدارة وفصل أفراد الأسرة مقصورة على الجمعية المسجلة للملف ({existing.RegisteredByCharityName})."
            );
        }

        var member = await _familyMemberRepo.FirstOrDefaultAsync(m => m.Id == dto.FamilyMemberId && m.BeneficiaryId == beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(BeneficiaryFamilyMember), dto.FamilyMemberId);

        // Delete member from current beneficiary's family
        await _familyMemberRepo.DeleteAsync(member);

        // If requested to create a brand new independent Beneficiary file for this person
        if (dto.CreateIndependentBeneficiaryProfile)
        {
            var charityId = _currentUserService.CharityId ?? "c1";
            var charityName = _currentUserService.CharityName ?? "الجمعية المسجلة";

            var newBeneficiary = new Beneficiary
            {
                Id = Guid.NewGuid().ToString(),
                NationalId = member.NationalId,
                Name = member.Name,
                Phone = existing.Phone,
                Category = "حالات منفصلة / مستقلة",
                MaritalStatus = "أعزب",
                FamilyCount = 1,
                Address = existing.Address,
                Housing = existing.Housing,
                HealthIssues = member.HealthStatus,
                ResearchNotes = $"تم فصل الحالة واستقلالها من ملف الأسرة ({existing.Name}). سبب الفصل: {dto.Reason ?? "استقلال بملف خاص"}",
                Status = "مستحق فعال",
                IncomeSource = member.IncomeSource,
                MonthlyIncome = member.IncomeAmount,
                RegisteredByCharityId = charityId,
                RegisteredByCharityName = charityName
            };

            await _beneficiaryRepo.AddAsync(newBeneficiary);
        }

        existing.FamilyCount = Math.Max(1, existing.FamilyCount - 1);
        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var remainingMembers = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == beneficiaryId);
        existing.FamilyMembers = remainingMembers.ToList();

        return existing.ToDto(currentCharityId);
    }

    public async Task<BeneficiaryDto> TransferHeadshipAsync(string beneficiaryId, TransferHeadshipDto dto)
    {
        var existing = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var currentCharityName = _currentUserService.CharityName ?? "الجمعية المسجلة";

        if (!string.IsNullOrEmpty(existing.RegisteredByCharityId) && existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: نقل ولاية الملف مقصورة على الجمعية المسجلة له ({existing.RegisteredByCharityName})."
            );
        }

        var newHeadMember = await _familyMemberRepo.FirstOrDefaultAsync(m => m.Id == dto.NewHeadFamilyMemberId && m.BeneficiaryId == beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(BeneficiaryFamilyMember), dto.NewHeadFamilyMemberId);

        // 1. تسجيل سجل التعاقب التاريخي للأسرة
        var history = new BeneficiaryHeadHistory
        {
            BeneficiaryId = existing.Id,
            PreviousHeadName = existing.Name,
            PreviousHeadNationalId = existing.NationalId,
            NewHeadName = newHeadMember.Name,
            NewHeadNationalId = newHeadMember.NationalId,
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "وفاة رب الأسرة السابق" : dto.Reason,
            Notes = dto.Notes ?? string.Empty,
            TransferredAt = DateTime.UtcNow,
            TransferredByCharityId = currentCharityId,
            TransferredByCharityName = currentCharityName
        };
        await _headHistoryRepo.AddAsync(history);

        // 2. تحويل العائل السابق إلى فرد أسرة (عائل سابق / متوفى) للحفاظ على سجله داخل الملف
        var existingFormerMember = await _familyMemberRepo.FirstOrDefaultAsync(
            m => m.BeneficiaryId == existing.Id && m.NationalId == existing.NationalId
        );

        if (existingFormerMember != null)
        {
            existingFormerMember.IsDeleted = false;
            existingFormerMember.DeletedAt = null;
            existingFormerMember.IsFormerHead = true;
            existingFormerMember.Name = existing.Name;
            existingFormerMember.Relationship = string.IsNullOrWhiteSpace(dto.FormerHeadRelationship) ? "رب الأسرة السابق (متوفى)" : dto.FormerHeadRelationship;
            existingFormerMember.Notes = $"عائل الأسرة السابق. سبب نقل الولاية: {history.Reason}";
            await _familyMemberRepo.UpdateAsync(existingFormerMember);
        }
        else
        {
            var formerHeadMember = new BeneficiaryFamilyMember
            {
                BeneficiaryId = existing.Id,
                NationalId = existing.NationalId,
                Name = existing.Name,
                Relationship = string.IsNullOrWhiteSpace(dto.FormerHeadRelationship) ? "رب الأسرة السابق (متوفى)" : dto.FormerHeadRelationship,
                MemberType = "معال",
                SupportType = "سابق / متوفى",
                Notes = $"عائل الأسرة السابق. سبب نقل الولاية: {history.Reason}",
                IsFormerHead = true,
                EducationStage = "غير محدد",
                IsSchoolDropout = false,
                IsChildLabor = false
            };
            await _familyMemberRepo.AddAsync(formerHeadMember);
        }

        // 3. ترقية الفرد المختار: إخراجه من جدول الأفراد التابعين لأنه ارتقى وأصبح هو صاحب الملف الأساسي
        await _familyMemberRepo.HardDeleteAsync(newHeadMember);

        // 4. تحديث البيانات الأساسية للمستفيد بالاسم والرقم القومي للعائل الجديد
        existing.Name = newHeadMember.Name;
        existing.NationalId = newHeadMember.NationalId;

        if (!string.IsNullOrWhiteSpace(dto.NewFamilyName))
        {
            existing.FamilyName = dto.NewFamilyName.Trim();
        }
        else if (history.Reason.Contains("وفاة") && !existing.FamilyName.Contains("المرحوم"))
        {
            existing.FamilyName = $"عائلة المرحوم {history.PreviousHeadName}";
        }

        // إذا كان الملف موقوفاً بسبب وفاة العائل السابق، يتم فك الإيقاف واستئناف الصرف للعائل الجديد
        if (existing.IsSuspended && (existing.SuspensionReasonTitle?.Contains("وفاة") == true || history.Reason.Contains("وفاة")))
        {
            existing.IsSuspended = false;
            existing.Status = "مستحق فعال";
            existing.SuspensionNotes = (existing.SuspensionNotes ?? "") + $" [تم استئناف النشاط ونقل الولاية للعائل الجديد ({newHeadMember.Name}) بتاريخ {DateTime.Now:yyyy-MM-dd}]";
        }

        existing.ResearchNotes = (existing.ResearchNotes ?? "") + $"\n[تم نقل ولاية الأسرة بتاريخ {DateTime.Now:yyyy-MM-dd} من ({history.PreviousHeadName}) إلى ({newHeadMember.Name}). السبب: {history.Reason}]";

        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var remainingMembers = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == beneficiaryId);
        existing.FamilyMembers = remainingMembers.ToList();

        var histories = await _headHistoryRepo.FindAsync(h => h.BeneficiaryId == beneficiaryId);
        existing.HeadHistories = histories.ToList();

        return existing.ToDto(currentCharityId);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        if (!string.IsNullOrEmpty(existing.RegisteredByCharityId) && existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: حذف أو أرشفة ملف الحالة مقصورة على الجمعية المسجلة له ({existing.RegisteredByCharityName})."
            );
        }

        // حذف لين (Soft Delete): يغير IsDeleted = true ويحفظ كافة البيانات التاريخية وحركات الصرف
        existing.Status = "مؤرشف / ملغي";
        await _beneficiaryRepo.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task ValidateNationalIdsAsync(string? currentBeneficiaryId, string beneficiaryNationalId, List<CreateFamilyMemberDto>? familyMembers)
    {
        beneficiaryNationalId = beneficiaryNationalId.Trim();

        if (string.IsNullOrWhiteSpace(beneficiaryNationalId))
        {
            throw new ValidationDomainException("الرقم القومي لصاحب الملف مطلوب!");
        }

        // 1. فحص عدم تكرار الرقم القومي لصاحب الملف مع أي مستفيد آخر
        var existingBen = await _beneficiaryRepo.FirstOrDefaultAsync(b => b.NationalId == beneficiaryNationalId && b.Id != currentBeneficiaryId);
        if (existingBen != null)
        {
            throw new ValidationDomainException($"الرقم القومي ({beneficiaryNationalId}) مسجل مسبقاً لمستفيد آخر باسم: {existingBen.Name}!");
        }

        // 2. فحص عدم تسجيل الرقم القومي لصاحب الملف كفرد أسرة تابع لأي مستفيد
        var existingMemberWithSameId = await _familyMemberRepo.FirstOrDefaultAsync(m => m.NationalId == beneficiaryNationalId && m.BeneficiaryId != currentBeneficiaryId);
        if (existingMemberWithSameId != null)
        {
            throw new ValidationDomainException($"الرقم القومي ({beneficiaryNationalId}) مسجل مسبقاً كفرد أسرة في ملف مستفيد آخر باسم: {existingMemberWithSameId.Name}!");
        }

        if (familyMembers != null && familyMembers.Count > 0)
        {
            var memberIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var member in familyMembers)
            {
                var mNatId = member.NationalId?.Trim();
                if (string.IsNullOrWhiteSpace(mNatId))
                {
                    throw new ValidationDomainException($"الرقم القومي لفرد الأسرة ({member.Name}) مطلوب!");
                }

                // منع تطابق الرقم القومي للفرد مع الرقم القومي لصاحب الملف نفسه
                if (string.Equals(mNatId, beneficiaryNationalId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationDomainException($"لا يمكن أن يتطابق الرقم القومي لفرد الأسرة ({member.Name}) مع الرقم القومي لصاحب الملف!");
                }

                // منع تكرار الرقم القومي داخل قائمة أفراد الأسرة في نفس الاستمارة
                if (!memberIds.Add(mNatId))
                {
                    throw new ValidationDomainException($"الرقم القومي ({mNatId}) مكرر أكثر من مرة بين أفراد الأسرة المسجلين!");
                }

                // فحص عدم تسجيل فرد الأسرة كصاحب ملف مستقل مسبقاً
                var benConflict = await _beneficiaryRepo.FirstOrDefaultAsync(b => b.NationalId == mNatId && b.Id != currentBeneficiaryId);
                if (benConflict != null)
                {
                    throw new ValidationDomainException($"الرقم القومي لفرد الأسرة ({member.Name} - {mNatId}) مسجل مسبقاً كصاحب ملف مستقل باسم: {benConflict.Name}!");
                }

                // فحص عدم تسجيل فرد الأسرة في أسرة مستفيد آخر
                var otherFamConflict = await _familyMemberRepo.FirstOrDefaultAsync(m => m.NationalId == mNatId && m.BeneficiaryId != currentBeneficiaryId);
                if (otherFamConflict != null)
                {
                    throw new ValidationDomainException($"الرقم القومي ({mNatId}) لفرد الأسرة ({member.Name}) مسجل مسبقاً كفرد أسرة في ملف مستفيد آخر!");
                }
            }
        }
    }
}
