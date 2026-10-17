using Charity.Application.DTOs.Distributions;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class DistributionService : IDistributionService
{
    private readonly IRepository<Distribution> _distributionRepo;
    private readonly IRepository<InventoryItem> _inventoryRepo;
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly IRepository<ExpenseCategory> _expenseCategoryRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DistributionService(
        IRepository<Distribution> distributionRepo,
        IRepository<InventoryItem> inventoryRepo,
        IRepository<Beneficiary> beneficiaryRepo,
        IRepository<ExpenseCategory> expenseCategoryRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _distributionRepo = distributionRepo;
        _inventoryRepo = inventoryRepo;
        _beneficiaryRepo = beneficiaryRepo;
        _expenseCategoryRepo = expenseCategoryRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DistributionDto>> GetMyCharityDistributionsAsync()
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var myDistributions = await _distributionRepo.FindAsync(d => d.CharityId == currentCharityId);
        return myDistributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();
    }

    public async Task<IReadOnlyList<DistributionDto>> GetAllNetworkDistributionsAsync()
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var allDistributions = await _distributionRepo.GetAllAsync();
        return allDistributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();
    }

    public async Task<IReadOnlyList<DistributionDto>> GetDistributionsByBeneficiaryIdAsync(string beneficiaryId)
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var distributions = await _distributionRepo.FindAsync(d => d.BeneficiaryId == beneficiaryId);
        return distributions.OrderByDescending(d => d.Date).Select(d => d.ToDto(currentCharityId)).ToList();
    }

    public async Task<BeneficiaryAidSummaryDto> GetBeneficiaryAidSummaryAsync(string beneficiaryId)
    {
        var currentCharityId = _currentUserService.CharityId ?? string.Empty;
        var beneficiary = await _beneficiaryRepo.GetByIdAsync(beneficiaryId)
            ?? throw new EntityNotFoundException(nameof(Beneficiary), beneficiaryId);

        var distributions = await _distributionRepo.FindAsync(d => d.BeneficiaryId == beneficiaryId);
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

        if (dto.Kind == "إعانة نقدية")
        {
            if (string.IsNullOrWhiteSpace(dto.ExpenseCategoryId))
            {
                throw new ValidationDomainException("يجب اختيار بند ونوع المصروف عند صرف مساعدة نقدية.");
            }

            var expenseCategory = await _expenseCategoryRepo.GetByIdAsync(dto.ExpenseCategoryId)
                ?? throw new EntityNotFoundException(nameof(ExpenseCategory), dto.ExpenseCategoryId);

            entity.ExpenseCategoryId = expenseCategory.Id;
            entity.ExpenseCategoryName = expenseCategory.Name;

            if (string.IsNullOrWhiteSpace(entity.ItemName))
            {
                entity.ItemName = expenseCategory.Name;
            }
        }

        if (string.IsNullOrWhiteSpace(entity.ReceivedByName))
        {
            entity.ReceivedByName = beneficiary.Name;
            entity.ReceivedByNationalId = beneficiary.NationalId;
            entity.ReceivedByRelationship = "صاحب الملف (رب الأسرة)";
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
                        throw new ValidationDomainException(
                            $"الكمية المطلوبة ({dto.Quantity}) أكبر من الرصيد المتوفر بالمخزن ({item.Quantity} {item.Unit})!"
                        );
                    }
                    item.Quantity = Math.Max(0, item.Quantity - dto.Quantity);
                    await _inventoryRepo.UpdateAsync(item);
                }
            }
        }

        await _distributionRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto(charityId);
    }
}
