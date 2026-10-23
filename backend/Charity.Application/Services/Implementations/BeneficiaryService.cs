using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;
using Charity.Domain.Security;
using Charity.Domain.Validation;

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

    private void EnsureViewPermission()
    {
        if (!_currentUserService.HasPermission(AppPermissions.BeneficiariesView))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية استعراض ملفات المستفيدين.");
    }

    private void EnsureEditPermission()
    {
        if (!_currentUserService.HasPermission(AppPermissions.BeneficiariesEdit))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية تعديل ملفات المستفيدين.");
    }

    private void MaskSensitiveDataIfRestricted(BeneficiaryDto dto)
    {
        if (_currentUserService.HasPermission(AppPermissions.BeneficiariesViewFull))
            return;

        dto.DebtNotes = "🔒 بيانات محجوبة (صلاحية خاصة)";
        dto.TotalDebtAmount = null;
        dto.DebtCreditor = "🔒 محجوب";
        dto.HealthIssues = "🔒 بيانات طبية خاصة محجوبة";
        dto.ChronicDiseaseDetails = "🔒 محجوب";
        dto.ResearchNotes = "🔒 تقرير البحث الاجتماعي محجوب لدواعي الخصوصية";
        dto.MonthlyIncome = null;
        dto.IncomeSource = "🔒 محجوب";
        dto.LandOrPropertyDetails = "🔒 محجوب";
        dto.GovernmentPensionDetails = "🔒 محجوب";
    }

    public async Task<IReadOnlyList<BeneficiaryDto>> GetAllAsync()
    {
        EnsureViewPermission();
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

        var dtoList = entities.Select(b => b.ToDto(currentCharityId)).ToList();
        foreach (var dto in dtoList)
        {
            MaskSensitiveDataIfRestricted(dto);
        }
        return dtoList;
    }

    public async Task<BeneficiaryDto?> GetByIdAsync(string id)
    {
        EnsureViewPermission();
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var entity = await _beneficiaryRepo.GetByIdAsync(id);
        if (entity == null) return null;

        var members = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == id);
        entity.FamilyMembers = members.ToList();

        var histories = await _headHistoryRepo.FindAsync(h => h.BeneficiaryId == id);
        entity.HeadHistories = histories.ToList();

        var dto = entity.ToDto(currentCharityId);
        MaskSensitiveDataIfRestricted(dto);
        return dto;
    }

    public async Task<BeneficiaryDto> CreateAsync(CreateBeneficiaryDto dto)
    {
        if (!_currentUserService.HasPermission(AppPermissions.BeneficiariesCreate))
        {
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية فتح ملفات مستفيدين جديدة.");
        }

        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId))
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكن إضافة مستفيد بدون جمعية مسجلة.");
        }
        var charityName = _currentUserService.CharityName ?? "الجمعية المسجلة";

        // التدقيق في عدم تكرار الرقم القومي سواء لصاحب الملف أو لأفراد الأسرة
        dto.Name = ValidatePersonDetails(dto.Name, dto.MonthlyIncome, "اسم صاحب الملف");
        ValidateFamilyCount(dto.FamilyCount);
        await ValidatePrivateLookupsAsync(dto.VillageId, dto.VillageAreaId, dto.AddressId);
        dto.NationalId = EgyptianNationalId.Validate(dto.NationalId, "الرقم القومي لصاحب الملف");
        await ValidateNationalIdsAsync(null, dto.NationalId, dto.FamilyMembers);

        var entity = dto.ToEntity(charityId, charityName);

        // التوليد التلقائي الموحد لكود العائلة بالباترن FAM-YY-0001
        if (string.IsNullOrWhiteSpace(entity.FamilyCode) || entity.FamilyCode!.StartsWith("FAM-", StringComparison.OrdinalIgnoreCase))
        {
            entity.FamilyCode = await GetNextFamilyCodeAsync();
        }
        else
        {
            entity.FamilyCode = dto.FamilyCode.Trim();
        }

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
        if (!_currentUserService.HasPermission(AppPermissions.BeneficiariesEdit))
        {
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية تعديل بيانات المستفيدين.");
        }

        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;


        // Strict Authorization Rule: Only the registering charity can update the beneficiary profile.
        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: لا تملك صلاحية تعديل بيانات هذا المستفيد. التعديل مقصور قانونياً على الجمعية التي قامت بفتح الملف ({existing.RegisteredByCharityName})."
            );
        }

        // التدقيق في عدم تكرار الرقم القومي
        dto.Name = ValidatePersonDetails(dto.Name, dto.MonthlyIncome, "اسم صاحب الملف");
        ValidateFamilyCount(dto.FamilyCount);
        await ValidatePrivateLookupsAsync(dto.VillageId, dto.VillageAreaId, dto.AddressId);
        dto.NationalId = EgyptianNationalId.Validate(dto.NationalId, "الرقم القومي لصاحب الملف");
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
        existing.GovernorateId = string.IsNullOrWhiteSpace(dto.GovernorateId) ? null : dto.GovernorateId;
        existing.GovernorateName = dto.GovernorateName;
        existing.DistrictId = string.IsNullOrWhiteSpace(dto.DistrictId) ? null : dto.DistrictId;
        existing.DistrictName = dto.DistrictName;
        existing.VillageId = string.IsNullOrWhiteSpace(dto.VillageId) ? null : dto.VillageId;
        existing.VillageName = dto.VillageName;
        existing.VillageAreaId = string.IsNullOrWhiteSpace(dto.VillageAreaId) ? null : dto.VillageAreaId;
        existing.VillageAreaName = dto.VillageAreaName;
        existing.AddressId = string.IsNullOrWhiteSpace(dto.AddressId) ? null : dto.AddressId;
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
                var previousMember = existingMembers.FirstOrDefault(m => m.Id == fmEntity.Id);
                if (previousMember != null)
                {
                    fmEntity.CreatedAt = previousMember.CreatedAt;
                    fmEntity.CreatedBy = previousMember.CreatedBy;
                    fmEntity.CreatedByName = previousMember.CreatedByName;
                    await _familyMemberRepo.UpdateAsync(fmEntity);
                }
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
        EnsureViewPermission();
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
        EnsureEditPermission();
        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;


        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: إيقاف الملف مقصور على الجمعية المسجلة له ({existing.RegisteredByCharityName})."
            );
        }

        if (!string.IsNullOrWhiteSpace(dto.SuspensionReasonId))
        {
            var reason = await _unitOfWork.Repository<SuspensionReason>().GetByIdAsync(dto.SuspensionReasonId)
                ?? throw new EntityNotFoundException(nameof(SuspensionReason), dto.SuspensionReasonId);
            TenantAccess.DemandOwner(reason.CharityId, _currentUserService);
            if (!reason.IsActive) throw new ValidationDomainException("سبب الإيقاف غير فعال.");
            dto.SuspensionReasonTitle = reason.Title;
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
        EnsureEditPermission();
        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;


        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
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
        EnsureEditPermission();
        var existing = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;


        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: إدارة وفصل أفراد الأسرة مقصورة على الجمعية المسجلة للملف ({existing.RegisteredByCharityName})."
            );
        }

        var member = await _familyMemberRepo.FirstOrDefaultAsync(m => m.Id == dto.FamilyMemberId && m.BeneficiaryId == beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(BeneficiaryFamilyMember), dto.FamilyMemberId);
        if (!string.IsNullOrWhiteSpace(dto.SeparationReasonId))
        {
            var reason = await _unitOfWork.Repository<SeparationReason>().GetByIdAsync(dto.SeparationReasonId)
                ?? throw new EntityNotFoundException(nameof(SeparationReason), dto.SeparationReasonId);
            TenantAccess.DemandOwner(reason.CharityId, _currentUserService);
            if (!reason.IsActive) throw new ValidationDomainException("سبب الانفصال غير فعال.");
            dto.Reason = reason.Title;
        }

        if (dto.CreateIndependentBeneficiaryProfile)
        {
            member.Name = ValidatePersonDetails(member.Name, member.IncomeAmount, "اسم الفرد المراد فصله");
            member.NationalId = EgyptianNationalId.Validate(member.NationalId, "الرقم القومي للفرد المراد فصله");
        }

        member.IsSeparated = true;
        member.SeparatedAt = DateTime.UtcNow;
        member.SeparationReason = dto.Reason;
        member.SeparationReasonId = dto.SeparationReasonId;

        // If requested to create a brand new independent Beneficiary file for this person
        if (dto.CreateIndependentBeneficiaryProfile)
        {
            var charityId = !string.IsNullOrWhiteSpace(_currentUserService.CharityId) ? _currentUserService.CharityId : (existing.RegisteredByCharityId ?? "c1");
            var charityName = !string.IsNullOrWhiteSpace(_currentUserService.CharityName) ? _currentUserService.CharityName : (existing.RegisteredByCharityName ?? "الجمعية المسجلة");

            var newBeneficiary = new Beneficiary
            {
                Id = Guid.NewGuid().ToString(),
                FamilyCode = await GetNextFamilyCodeAsync(),
                FamilyName = $"عائلة {member.Name}",
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
            member.SeparatedToBeneficiaryId = newBeneficiary.Id;
        }

        // Delete member from current beneficiary's family (soft delete frees the national ID for new independent headship)
        await _familyMemberRepo.DeleteAsync(member);

        existing.FamilyCount = Math.Max(1, existing.FamilyCount - 1);
        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var remainingMembers = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == beneficiaryId);
        existing.FamilyMembers = remainingMembers.ToList();

        return existing.ToDto(currentCharityId);
    }

    public async Task<FamilyMemberDto> AddFamilyMemberAsync(string beneficiaryId, CreateFamilyMemberDto dto)
    {
        EnsureEditPermission();
        var existing = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;


        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: إضافة أفراد مقصورة على الجمعية المسجلة للملف ({existing.RegisteredByCharityName})."
            );
        }

        var nid = EgyptianNationalId.Validate(dto.NationalId, "الرقم القومي لفرد الأسرة");
        dto.NationalId = nid;

        dto.Name = ValidatePersonDetails(dto.Name, dto.IncomeAmount, "اسم فرد الأسرة");

        var alreadyExistsInFamily = await _familyMemberRepo.FirstOrDefaultAsync(
            m => m.BeneficiaryId == beneficiaryId && m.NationalId == nid
        );
        if (alreadyExistsInFamily != null)
        {
            throw new ValidationDomainException("الرقم القومي مسجل بالفعل كفرد في هذه الأسرة.");
        }

        if (existing.NationalId == nid)
        {
            throw new ValidationDomainException("هذا الرقم القومي مسجل كصاحب الملف الأساسي حالياً.");
        }

        // Apply the same network-wide identity checks used when creating or editing a file.
        await ValidateNationalIdsAsync(beneficiaryId, existing.NationalId, [dto]);

        var member = dto.ToEntity(beneficiaryId);
        if (string.IsNullOrWhiteSpace(member.Id))
        {
            member.Id = "fm_" + Guid.NewGuid().ToString("N");
        }

        await _familyMemberRepo.AddAsync(member);

        var activeCount = await _familyMemberRepo.CountAsync(m => m.BeneficiaryId == beneficiaryId && !m.IsDeleted);
        existing.FamilyCount = Math.Max(existing.FamilyCount, activeCount + 2);
        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        return member.ToDto();
    }

    public async Task<BeneficiaryDto> TransferHeadshipAsync(string beneficiaryId, TransferHeadshipDto dto)
    {
        EnsureEditPermission();
        var existing = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var currentCharityName = _currentUserService.CharityName ?? (existing.RegisteredByCharityName ?? "الجمعية المسجلة");


        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException(
                $"غير مصرح: نقل ولاية الملف مقصورة على الجمعية المسجلة له ({existing.RegisteredByCharityName})."
            );
        }

        var newHeadMember = await _familyMemberRepo.FirstOrDefaultAsync(m => m.Id == dto.NewHeadFamilyMemberId && m.BeneficiaryId == beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(BeneficiaryFamilyMember), dto.NewHeadFamilyMemberId);

        bool keepFormerHeadInFamily = dto.FormerHeadAction != "RemoveFromFamily";
        newHeadMember.Name = ValidatePersonDetails(newHeadMember.Name, newHeadMember.IncomeAmount, "اسم العائل الجديد");
        newHeadMember.NationalId = EgyptianNationalId.Validate(newHeadMember.NationalId, "الرقم القومي للعائل الجديد");
        await ValidateNationalIdsAsync(beneficiaryId, newHeadMember.NationalId, null);
        if (keepFormerHeadInFamily)
            existing.NationalId = EgyptianNationalId.Validate(existing.NationalId, "الرقم القومي للعائل السابق");
        bool isDeath = (dto.Reason ?? string.Empty).Contains("وفاة");
        var actionDescription = keepFormerHeadInFamily ? "نقل ولاية مع بقاء العائل السابق بالأسرة (حي ومعال)" : "نقل ولاية مع حذف واستبعاد العائل السابق من الأسرة";

        // 1. تسجيل سجل التعاقب التاريخي للأسرة
        var history = new BeneficiaryHeadHistory
        {
            BeneficiaryId = existing.Id,
            PreviousHeadName = existing.Name,
            PreviousHeadNationalId = existing.NationalId,
            NewHeadName = newHeadMember.Name,
            NewHeadNationalId = newHeadMember.NationalId,
            Reason = string.IsNullOrWhiteSpace(dto.Reason) ? (keepFormerHeadInFamily ? "عجز أو ظروف خاصة (مع بقائه بالأسرة)" : "وفاة أو خروج من الأسرة") : dto.Reason,
            Notes = $"[{actionDescription}] " + (dto.Notes ?? string.Empty).Trim(),
            TransferredAt = DateTime.UtcNow,
            TransferredByCharityId = currentCharityId,
            TransferredByCharityName = currentCharityName
        };
        await _headHistoryRepo.AddAsync(history);

        // 2. معالجة العائل السابق بناءً على الاختيار (بقاء كفرد معال حي بالأسرة vs حذف واستبعاد)
        var existingFormerMember = await _familyMemberRepo.FirstOrDefaultAsync(
            m => m.BeneficiaryId == existing.Id && m.NationalId == existing.NationalId
        );

        if (keepFormerHeadInFamily)
        {
            string defaultRelationship = !string.IsNullOrWhiteSpace(dto.FormerHeadRelationship) 
                ? dto.FormerHeadRelationship 
                : "زوج (عائل سابق / مريض معال)";
            string defaultSupportType = !string.IsNullOrWhiteSpace(dto.FormerHeadSupportType) 
                ? dto.FormerHeadSupportType 
                : "مريض / عاجز عن العمل";

            if (existingFormerMember != null)
            {
                existingFormerMember.IsDeleted = false;
                existingFormerMember.DeletedAt = null;
                existingFormerMember.IsFormerHead = true;
                existingFormerMember.Name = existing.Name;
                existingFormerMember.Relationship = defaultRelationship;
                existingFormerMember.MemberType = "معال";
                existingFormerMember.SupportType = defaultSupportType;
                existingFormerMember.Notes = $"عائل الأسرة السابق (حي ومقيم بالأسرة كمعال). سبب نقل الولاية: {history.Reason}";
                await _familyMemberRepo.UpdateAsync(existingFormerMember);
            }
            else
            {
                var formerHeadMember = new BeneficiaryFamilyMember
                {
                    BeneficiaryId = existing.Id,
                    NationalId = existing.NationalId,
                    Name = existing.Name,
                    Relationship = defaultRelationship,
                    MemberType = "معال",
                    SupportType = defaultSupportType,
                    Notes = $"عائل الأسرة السابق (حي ومقيم بالأسرة كمعال). سبب نقل الولاية: {history.Reason}",
                    IsFormerHead = true,
                    EducationStage = "غير محدد",
                    IsSchoolDropout = false,
                    IsChildLabor = false,
                    IsDeleted = false
                };
                await _familyMemberRepo.AddAsync(formerHeadMember);
            }
        }
        else
        {
            // حذف / استبعاد العائل السابق من جدول أفراد الأسرة النشطين
            if (existingFormerMember != null)
            {
                await _familyMemberRepo.DeleteAsync(existingFormerMember); // Soft delete
            }
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
        else if (!keepFormerHeadInFamily && isDeath && !existing.FamilyName.Contains("المرحوم"))
        {
            existing.FamilyName = $"عائلة المرحوم {history.PreviousHeadName}";
        }
        else if (keepFormerHeadInFamily && existing.FamilyName.Contains("المرحوم"))
        {
            existing.FamilyName = existing.FamilyName.Replace("المرحوم ", "").Trim();
        }
        else if (string.IsNullOrWhiteSpace(existing.FamilyName))
        {
            existing.FamilyName = $"عائلة {history.PreviousHeadName}";
        }

        // إذا كان الملف موقوفاً بسبب وفاة العائل السابق، يتم فك الإيقاف واستئناف الصرف للعائل الجديد
        if (existing.IsSuspended && (existing.SuspensionReasonTitle?.Contains("وفاة") == true || history.Reason.Contains("وفاة")))
        {
            existing.IsSuspended = false;
            existing.Status = "مستحق فعال";
            existing.SuspensionNotes = (existing.SuspensionNotes ?? "") + $" [تم استئناف النشاط ونقل الولاية للعائل الجديد ({newHeadMember.Name}) بتاريخ {DateTime.Now:yyyy-MM-dd}]";
        }

        existing.ResearchNotes = (existing.ResearchNotes ?? "") + $"\n[تم نقل ولاية الأسرة بتاريخ {DateTime.Now:yyyy-MM-dd} من ({history.PreviousHeadName}) إلى ({newHeadMember.Name}). " +
            $"موقف العائل السابق: {(keepFormerHeadInFamily ? "بقاء بالأسرة كمعال حي ومقيم" : "حذف واستبعاد من أفراد الأسرة")}. السبب: {history.Reason}]";

        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        // تحديث عدد أفراد الأسرة الفعلي
        var activeCount = await _familyMemberRepo.CountAsync(m => m.BeneficiaryId == beneficiaryId && !m.IsDeleted);
        existing.FamilyCount = Math.Max(1, activeCount + 1);
        await _beneficiaryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        var remainingMembers = await _familyMemberRepo.FindAsync(m => m.BeneficiaryId == beneficiaryId && !m.IsDeleted);
        existing.FamilyMembers = remainingMembers.ToList();

        var histories = await _headHistoryRepo.FindAsync(h => h.BeneficiaryId == beneficiaryId);
        existing.HeadHistories = histories.ToList();

        return existing.ToDto(currentCharityId);
    }

    public async Task DeleteAsync(string id)
    {
        if (!_currentUserService.HasPermission(AppPermissions.BeneficiariesDelete))
        {
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية حذف ملفات المستفيدين.");
        }

        var existing = await _beneficiaryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), id);

        var currentCharityId = _currentUserService.CharityId ?? string.Empty;


        if (string.IsNullOrWhiteSpace(currentCharityId) || existing.RegisteredByCharityId != currentCharityId)
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

    private async Task ValidatePrivateLookupsAsync(string? villageId, string? areaId, string? addressId)
    {
        if (!string.IsNullOrWhiteSpace(villageId))
        {
            var village = await _unitOfWork.Repository<Village>().GetByIdAsync(villageId)
                ?? throw new EntityNotFoundException(nameof(Village), villageId);
            TenantAccess.DemandOwner(village.CharityId, _currentUserService);
        }
        if (!string.IsNullOrWhiteSpace(areaId))
        {
            var area = await _unitOfWork.Repository<VillageArea>().GetByIdAsync(areaId)
                ?? throw new EntityNotFoundException(nameof(VillageArea), areaId);
            TenantAccess.DemandOwner(area.CharityId, _currentUserService);
        }
        if (!string.IsNullOrWhiteSpace(addressId))
        {
            var address = await _unitOfWork.Repository<AddressRecord>().GetByIdAsync(addressId)
                ?? throw new EntityNotFoundException(nameof(AddressRecord), addressId);
            TenantAccess.DemandOwner(address.CharityId, _currentUserService);
        }
    }

    private static string ValidatePersonDetails(string? name, decimal? income, string nameField)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationDomainException($"{nameField} مطلوب ولا يمكن أن يكون فارغاً أو مسافات فقط.");
        if (income < 0)
            throw new ValidationDomainException("الدخل لا يمكن أن يكون سالباً؛ أدخل صفراً أو قيمة أكبر.");
        return name.Trim();
    }

    private static void ValidateFamilyCount(int familyCount)
    {
        if (familyCount < 1)
            throw new ValidationDomainException("عدد أفراد الأسرة يجب أن يكون واحداً على الأقل.");
    }

    private async Task ValidateNationalIdsAsync(string? currentBeneficiaryId, string beneficiaryNationalId, List<CreateFamilyMemberDto>? familyMembers)
    {
        beneficiaryNationalId = EgyptianNationalId.Validate(beneficiaryNationalId, "الرقم القومي لصاحب الملف");

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
                member.Name = ValidatePersonDetails(member.Name, member.IncomeAmount, "اسم فرد الأسرة");
                var mNatId = EgyptianNationalId.Validate(member.NationalId, $"الرقم القومي لفرد الأسرة ({member.Name})");
                member.NationalId = mNatId;

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

    public async Task<string> GetNextFamilyCodeAsync()
    {
        var year = DateTime.UtcNow.ToString("yy");
        var prefix = $"FAM-{year}-";
        var existingBeneficiaries = await _beneficiaryRepo.FindAsync(b => b.FamilyCode != null && b.FamilyCode.StartsWith(prefix));

        int maxSeq = 0;
        foreach (var b in existingBeneficiaries)
        {
            if (string.IsNullOrWhiteSpace(b.FamilyCode)) continue;
            var parts = b.FamilyCode.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int seq))
            {
                if (seq > maxSeq) maxSeq = seq;
            }
        }

        int nextSeq = maxSeq + 1;
        return $"{prefix}{nextSeq:D4}";
    }
}

