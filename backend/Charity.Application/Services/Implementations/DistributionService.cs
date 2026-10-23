using Charity.Application.DTOs.Distributions;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;
using Charity.Domain.Validation;

namespace Charity.Application.Services.Implementations;

public class DistributionService : IDistributionService
{
    private readonly IRepository<Distribution> _distributionRepo;
    private readonly IRepository<InventoryItem> _inventoryRepo;
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly IRepository<BeneficiaryFamilyMember> _familyMemberRepo;
    private readonly IRepository<ExpenseCategory> _expenseCategoryRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Donation> _donationRepo;

    public DistributionService(
        IRepository<Distribution> distributionRepo,
        IRepository<InventoryItem> inventoryRepo,
        IRepository<Beneficiary> beneficiaryRepo,
        IRepository<BeneficiaryFamilyMember> familyMemberRepo,
        IRepository<ExpenseCategory> expenseCategoryRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IRepository<Donation> donationRepo)
    {
        _distributionRepo = distributionRepo;
        _inventoryRepo = inventoryRepo;
        _beneficiaryRepo = beneficiaryRepo;
        _familyMemberRepo = familyMemberRepo;
        _expenseCategoryRepo = expenseCategoryRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _donationRepo = donationRepo;
    }

    public async Task<IReadOnlyList<DistributionDto>> GetMyCharityDistributionsAsync()
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var myDistributions = await _distributionRepo.FindAsync(d => d.CharityId == currentCharityId);
        return myDistributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();
    }

    public async Task<IReadOnlyList<DistributionDto>> GetAllNetworkDistributionsAsync()
    {
        if (_currentUserService.Role != "Admin")
            throw new UnauthorizedDomainException("عرض سجل الشبكة الكامل متاح لإدارة المنصة فقط. تاريخ الجمعيات الأخرى متاح داخل ملف المستفيد.");
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var allDistributions = await _distributionRepo.GetAllAsync();
        return allDistributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();
    }

    public async Task<IReadOnlyList<DistributionDto>> GetDistributionsByBeneficiaryIdAsync(string beneficiaryId)
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var distributions = await _distributionRepo.FindAsync(d => d.BeneficiaryId == beneficiaryId && d.CharityId == currentCharityId);
        return distributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();
    }

    public async Task<BeneficiaryAidSummaryDto> GetBeneficiaryAidSummaryAsync(string beneficiaryId)
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var beneficiary = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var distributions = await _distributionRepo.FindAsync(d => d.BeneficiaryId == beneficiaryId && d.CharityId == currentCharityId);
        var dtos = distributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();

        var otherCharitiesCount = dtos.Count(d => !d.IsFromCurrentCharity);

        return new BeneficiaryAidSummaryDto
        {
            BeneficiaryId = beneficiary.Id,
            BeneficiaryName = beneficiary.Name,
            TotalDistributionsCount = dtos.Count,
            OtherCharitiesDistributionsCount = otherCharitiesCount,
            AidHistory = dtos
        };
    }

    public async Task<DistributionDto> CreateDistributionAsync(CreateDistributionDto dto)
    {
        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId)) throw new UnauthorizedDomainException("لا يوجد حساب جمعية صالح.");
        if (!double.IsFinite(dto.Quantity) || dto.Quantity <= 0)
            throw new ValidationDomainException("كمية الصرف يجب أن تكون أكبر من صفر.");
        if (dto.Amount < 0) throw new ValidationDomainException("قيمة الصرف لا يمكن أن تكون سالبة.");
        var distributionDate = BusinessDate.ValidateTodayOrEarlier(dto.Date, "تاريخ الصرف");
        if (string.IsNullOrWhiteSpace(dto.InventoryItemId) && dto.Kind != "إعانة نقدية")
            throw new ValidationDomainException("اختيار صنف أو خدمة مطلوب.");
        var charityName = _currentUserService.CharityName ?? "الجمعية المانحة";

        var beneficiary = await _beneficiaryRepo.GetByIdAsync(dto.BeneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), dto.BeneficiaryId);

        // الحظر الصارم للصرف على الملف الموقوف
        if (beneficiary.IsSuspended || beneficiary.Status == "موقوف")
        {
            var reasonMsg = !string.IsNullOrEmpty(beneficiary.SuspensionReasonTitle) 
                ? $"بسبب: {beneficiary.SuspensionReasonTitle}" 
                : string.Empty;
            throw new ValidationDomainException(
                $"⛔ محظور الصرف: ملف هذا المستفيد ({beneficiary.Name}) موقوف حالياً وغير فعال {reasonMsg}! لا يمكن تسجيل أي حركة صرف له حتى يتم إعادة تفعيل الملف."
            );
        }

        var entity = dto.ToEntity(charityId, charityName);
        entity.Date = distributionDate;
        await SetVerifiedRecipientAsync(entity, dto, beneficiary);
        var warnings = new List<string>();
        decimal? remainingCash = null;
        InventoryItem? deductedItem = null;

        if (dto.Kind == "إعانة نقدية")
        {
            if (!dto.Amount.HasValue || dto.Amount <= 0)
                throw new ValidationDomainException("مبلغ الصرف النقدي يجب أن يكون أكبر من صفر.");
            if (string.IsNullOrWhiteSpace(dto.ExpenseCategoryId))
            {
                throw new ValidationDomainException("يجب اختيار بند ونوع المصروف عند صرف مساعدة نقدية.");
            }

            var expenseCategory = await _expenseCategoryRepo.GetByIdAsync(dto.ExpenseCategoryId)
                ?? throw new EntityNotFoundException(nameof(ExpenseCategory), dto.ExpenseCategoryId);
            TenantAccess.DemandOwner(expenseCategory.CharityId, _currentUserService);
            if (!expenseCategory.IsActive)
                throw new ValidationDomainException("بند المصروف المختار غير فعال.");

            entity.ExpenseCategoryId = expenseCategory.Id;
            entity.ExpenseCategoryName = expenseCategory.Name;

            if (string.IsNullOrWhiteSpace(entity.ItemName))
            {
                entity.ItemName = expenseCategory.Name;
            }
            remainingCash = await CheckCashBalanceAsync(charityId, dto.Amount.Value, dto.AllowNegativeBalance, warnings);
        }

        // If tangible inventory item, auto deduct from active charity's warehouse
        if (!string.IsNullOrEmpty(dto.InventoryItemId))
        {
            var item = await _inventoryRepo.GetByIdAsync(dto.InventoryItemId)
                ?? throw new EntityNotFoundException(nameof(InventoryItem), dto.InventoryItemId);
            if (item != null)
            {
                // Ensure the item belongs to THIS charity
                if (item.CharityId != charityId)
                {
                    throw new UnauthorizedDomainException("لا يمكنك الصرف من مخزن يتبع جمعية أخرى!");
                }

                if (string.IsNullOrEmpty(entity.ItemName)) entity.ItemName = item.ItemName;
                if (string.IsNullOrEmpty(entity.Unit)) entity.Unit = item.Unit;
                entity.Kind = item.Kind;

                if (item.Kind != "خدمة")
                {
                    if (item.Quantity < dto.Quantity)
                    {
                        ConfirmOverdraft(dto.AllowNegativeBalance, warnings,
                            $"رصيد الصنف ({item.ItemName}) المسجل {item.Quantity} {item.Unit}، والمطلوب {dto.Quantity}. سيصبح الرصيد {item.Quantity - dto.Quantity} {item.Unit}. هل تريد الاستمرار وتسجيل العجز؟");
                    }
                    deductedItem = item;
                    _unitOfWork.ChangeInventoryBalance(item, -dto.Quantity, dto.AllowNegativeBalance);
                }
            }
        }

        await _distributionRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var result = entity.ToDto(charityId);
        result.Warnings = warnings;
        result.RemainingStock = deductedItem?.Quantity;
        if (deductedItem?.Quantity < 0 && warnings.Count == 0)
            warnings.Add($"تم تسجيل عجز في رصيد الصنف: {deductedItem.Quantity} {deductedItem.Unit}.");
        result.RemainingCashBalance = _unitOfWork.CashBalanceAfterDisbursement ?? remainingCash;
        if (result.RemainingCashBalance < 0 && warnings.Count == 0)
            warnings.Add($"تم تسجيل عجز نقدي: {result.RemainingCashBalance} جنيه.");
        return result;
    }

    public async Task<BulkDistributionResultDto> CreateBulkDistributionAsync(BulkDistributionRequestDto dto)
    {
        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId)) throw new UnauthorizedDomainException("لا يوجد حساب جمعية صالح.");
        var charityName = _currentUserService.CharityName ?? "الجمعية المانحة";

        if (dto.BeneficiaryIds == null || dto.BeneficiaryIds.Count == 0)
        {
            throw new ValidationDomainException("يرجى تحديد مستفيد واحد على الأقل لتنفيذ الصرف الجماعي.");
        }
        if (dto.AmountPerBeneficiary < 0)
            throw new ValidationDomainException("مبلغ حركة الصرف لا يمكن أن يكون سالباً؛ السالب مسموح في الرصيد المتبقي فقط.");
        var dateStr = BusinessDate.ValidateTodayOrEarlier(dto.Date, "تاريخ الصرف");
        if (dto.Kind != "إعانة نقدية" && string.IsNullOrWhiteSpace(dto.InventoryItemId))
            throw new ValidationDomainException("اختيار صنف أو خدمة مطلوب.");
        var warnings = new List<string>();
        decimal? remainingCash = null;

        if (dto.Kind == "إعانة نقدية")
        {
            if (dto.AmountPerBeneficiary == null || dto.AmountPerBeneficiary <= 0)
            {
                throw new ValidationDomainException("يرجى تحديد المبلغ النقدي لكل أسرة (أكبر من صفر).");
            }
            if (string.IsNullOrWhiteSpace(dto.ExpenseCategoryId))
            {
                throw new ValidationDomainException("يجب اختيار بند ونوع المصروف عند صرف مساعدات نقدية جماعية.");
            }
        }
        else
        {
            if (!double.IsFinite(dto.QuantityPerBeneficiary) || dto.QuantityPerBeneficiary <= 0)
            {
                throw new ValidationDomainException("كمية الصرف لكل أسرة يجب أن تكون أكبر من صفر.");
            }
        }

        ExpenseCategory? expenseCategory = null;
        if (!string.IsNullOrWhiteSpace(dto.ExpenseCategoryId))
        {
            expenseCategory = await _expenseCategoryRepo.GetByIdAsync(dto.ExpenseCategoryId)
                ?? throw new EntityNotFoundException(nameof(ExpenseCategory), dto.ExpenseCategoryId);
            TenantAccess.DemandOwner(expenseCategory.CharityId, _currentUserService);
            if (!expenseCategory.IsActive)
                throw new ValidationDomainException("بند المصروف المختار غير فعال.");
        }

        // Fetch beneficiaries
        var beneficiaries = new List<Beneficiary>();
        foreach (var id in dto.BeneficiaryIds.Distinct())
        {
            var b = await _beneficiaryRepo.GetByIdAsync(id);
            if (b != null) beneficiaries.Add(b);
        }

        if (beneficiaries.Count == 0)
        {
            throw new ValidationDomainException("لم يتم العثور على أي ملفات مستفيدين مطابقة.");
        }
        if (beneficiaries.Count != dto.BeneficiaryIds.Distinct().Count())
            throw new ValidationDomainException("بعض الأسر المحددة لم تعد موجودة. أعد تحميل القائمة قبل الصرف.");
        if (beneficiaries.Any(b => !MatchesTargetFilters(b, dto)))
            throw new ValidationDomainException("بعض الأسر المحددة خارج منطقة أو فلاتر خط السير. أعد تحديد أسر المنطقة قبل الصرف.");

        // Separate active vs suspended beneficiaries
        var activeBeneficiaries = beneficiaries.Where(b => !b.IsSuspended && b.Status != "موقوف").ToList();
        var skippedBeneficiaries = beneficiaries.Where(b => b.IsSuspended || b.Status == "موقوف").ToList();

        if (activeBeneficiaries.Count == 0)
        {
            throw new ValidationDomainException("جميع الحالات المحددة موقوفة ومحظورة من الصرف! لا يمكن إتمام التوزيع.");
        }
        if (dto.Kind == "إعانة نقدية")
            remainingCash = await CheckCashBalanceAsync(charityId, dto.AmountPerBeneficiary!.Value * activeBeneficiaries.Count,
                dto.AllowNegativeBalance, warnings);

        InventoryItem? inventoryItem = null;
        if (!string.IsNullOrEmpty(dto.InventoryItemId))
        {
            inventoryItem = await _inventoryRepo.GetByIdAsync(dto.InventoryItemId)
                ?? throw new EntityNotFoundException(nameof(InventoryItem), dto.InventoryItemId);

            if (inventoryItem.CharityId != charityId)
            {
                throw new UnauthorizedDomainException("لا يمكنك الصرف من مخزن يتبع جمعية أخرى!");
            }

            if (inventoryItem.Kind != "خدمة")
            {
                var totalNeeded = dto.QuantityPerBeneficiary * activeBeneficiaries.Count;
                if (inventoryItem.Quantity < totalNeeded)
                {
                    ConfirmOverdraft(dto.AllowNegativeBalance, warnings,
                        $"رصيد الصنف ({inventoryItem.ItemName}) المسجل {inventoryItem.Quantity} {inventoryItem.Unit}، والمطلوب للتوزيع {totalNeeded}. سيصبح الرصيد {inventoryItem.Quantity - totalNeeded} {inventoryItem.Unit}. هل تريد الاستمرار وتسجيل العجز؟");
                }

                _unitOfWork.ChangeInventoryBalance(inventoryItem, -totalNeeded, dto.AllowNegativeBalance);
            }
        }

        var createdEntities = new List<Distribution>();
        var baseItemName = inventoryItem?.ItemName ?? expenseCategory?.Name ?? dto.ItemName ?? "مساعدة غذائية / عينية";
        var baseUnit = inventoryItem?.Unit ?? dto.Unit ?? "وحدة";
        var baseKind = inventoryItem?.Kind ?? dto.Kind;

        foreach (var b in activeBeneficiaries)
        {
            var notesText = dto.Notes;
            if (string.IsNullOrWhiteSpace(notesText))
            {
                var areaPart = !string.IsNullOrWhiteSpace(dto.TargetAreaName) ? $"منطقة: {dto.TargetAreaName}" : string.Empty;
                var volPart = !string.IsNullOrWhiteSpace(dto.VolunteerOrWorkerName) ? $"المتطوع/المندوب: {dto.VolunteerOrWorkerName}" : string.Empty;
                notesText = $"توزيع ميداني جماعي {areaPart} {volPart}".Trim();
            }

            var dist = new Distribution
            {
                BeneficiaryId = b.Id,
                InventoryItemId = inventoryItem?.Id,
                ItemName = baseItemName,
                Kind = baseKind,
                Quantity = dto.QuantityPerBeneficiary,
                Unit = baseUnit,
                Amount = dto.AmountPerBeneficiary,
                ExpenseCategoryId = expenseCategory?.Id,
                ExpenseCategoryName = expenseCategory?.Name,
                Date = dateStr,
                Notes = notesText,
                ReceivedByName = b.Name,
                ReceivedByNationalId = b.NationalId,
                ReceivedByRelationship = "صاحب الملف (رب الأسرة)",
                CharityId = charityId,
                CharityName = charityName
            };

            await _distributionRepo.AddAsync(dist);
            createdEntities.Add(dist);
        }

        await _unitOfWork.SaveChangesAsync();

        if (inventoryItem?.Kind != "خدمة" && inventoryItem?.Quantity < 0 && warnings.Count == 0)
            warnings.Add($"تم تسجيل عجز في رصيد الصنف: {inventoryItem.Quantity} {inventoryItem.Unit}.");
        remainingCash = _unitOfWork.CashBalanceAfterDisbursement ?? remainingCash;
        if (remainingCash < 0 && warnings.Count == 0)
            warnings.Add($"تم تسجيل عجز نقدي: {remainingCash} جنيه.");
        return new BulkDistributionResultDto
        {
            Warnings = warnings,
            RemainingCashBalance = remainingCash,
            SuccessCount = createdEntities.Count,
            SkippedCount = skippedBeneficiaries.Count,
            TotalQuantityDisbursed = createdEntities.Sum(d => d.Quantity),
            TotalAmountDisbursed = createdEntities.Sum(d => d.Amount ?? 0),
            InventoryItemName = baseItemName,
            RemainingStock = inventoryItem?.Quantity ?? 0,
            VolunteerOrWorkerName = dto.VolunteerOrWorkerName,
            VolunteerPhone = dto.VolunteerPhone,
            TargetAreaName = dto.TargetAreaName,
            Date = dateStr,
            SkippedBeneficiaryNames = skippedBeneficiaries.Select(b => $"{b.Name} ({b.FamilyCode}) - {b.SuspensionReasonTitle ?? "موقوف"}").ToList(),
            CreatedDistributions = createdEntities.Select(d => d.ToDto(charityId)).ToList()
        };
    }

    private static bool MatchesTargetFilters(Beneficiary beneficiary, BulkDistributionRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.TargetGovernorateId) &&
            beneficiary.GovernorateId != request.TargetGovernorateId) return false;
        if (!string.IsNullOrWhiteSpace(request.TargetDistrictId) &&
            beneficiary.DistrictId != request.TargetDistrictId) return false;
        if (!string.IsNullOrWhiteSpace(request.TargetPriorityTier) &&
            beneficiary.PriorityTier != request.TargetPriorityTier) return false;

        var term = request.TargetAreaFilter?.Trim();
        if (string.IsNullOrEmpty(term)) return true;
        return new[] { beneficiary.VillageAreaName, beneficiary.VillageName, beneficiary.VillageOrDistrict,
                beneficiary.Address, beneficiary.DistrictName, beneficiary.GovernorateName }
            .Any(value => value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
    }

    private async Task SetVerifiedRecipientAsync(Distribution entity, CreateDistributionDto dto, Beneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(dto.ReceivedByMemberId))
        {
            if ((!string.IsNullOrWhiteSpace(dto.ReceivedByName) &&
                 !string.Equals(dto.ReceivedByName.Trim(), beneficiary.Name.Trim(), StringComparison.Ordinal)) ||
                (!string.IsNullOrWhiteSpace(dto.ReceivedByNationalId) &&
                 dto.ReceivedByNationalId.Trim() != beneficiary.NationalId))
                throw new ValidationDomainException("المستلم يجب أن يكون صاحب الملف أو فردًا مسجلًا في الأسرة.");

            entity.ReceivedByMemberId = null;
            entity.ReceivedByName = beneficiary.Name;
            entity.ReceivedByNationalId = beneficiary.NationalId;
            entity.ReceivedByRelationship = "صاحب الملف (رب الأسرة)";
            return;
        }

        var member = await _familyMemberRepo.GetByIdAsync(dto.ReceivedByMemberId);
        if (member == null || member.BeneficiaryId != beneficiary.Id || member.IsSeparated ||
            member.Relationship.Contains("متوفى") || member.SupportType.Contains("متوفى"))
            throw new ValidationDomainException("المستلم المختار ليس فردًا مؤهلًا في هذه الأسرة.");

        entity.ReceivedByMemberId = member.Id;
        entity.ReceivedByName = member.Name;
        entity.ReceivedByNationalId = member.NationalId;
        entity.ReceivedByRelationship = member.Relationship;
    }

    private static void ConfirmOverdraft(bool confirmed, List<string> warnings, string message)
    {
        if (!confirmed) throw new BalanceConfirmationRequiredException(message);
        warnings.Add(message);
    }

    private async Task<decimal> CheckCashBalanceAsync(string charityId, decimal requested, bool confirmed, List<string> warnings)
    {
        var donations = await _donationRepo.FindAsync(d => d.CharityId == charityId);
        var distributions = await _distributionRepo.FindAsync(d => d.CharityId == charityId && d.Kind == "إعانة نقدية");
        var available = donations.Where(d => d.DonationKind == "نقدي" || string.IsNullOrEmpty(d.DonationKind))
            .Sum(d => d.Amount ?? 0) - distributions.Sum(d => d.Amount ?? 0);
        var remaining = available - requested;
        if (remaining < 0)
            ConfirmOverdraft(confirmed, warnings,
                $"الرصيد النقدي المسجل {available} جنيه، والمطلوب {requested} جنيه. سيصبح الرصيد {remaining} جنيه. هل تريد الاستمرار وتسجيل العجز؟");
        _unitOfWork.CheckCashDisbursement(charityId, requested, confirmed);
        return remaining;
    }
}
