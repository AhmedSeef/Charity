using Charity.Application.DTOs.Donations;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class DonationService : IDonationService
{
    private readonly IRepository<Donation> _donationRepo;
    private readonly IRepository<InventoryItem> _inventoryRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DonationService(
        IRepository<Donation> donationRepo,
        IRepository<InventoryItem> inventoryRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _donationRepo = donationRepo;
        _inventoryRepo = inventoryRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DonationDto>> GetMyDonationsAsync()
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.DonationsView))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية استعراض سجل التبرعات.");

        var charityId = _currentUserService.CharityId ?? string.Empty;
        var donations = await _donationRepo.FindAsync(d => d.CharityId == charityId);
        return donations.OrderByDescending(d => d.Date).Select(d => d.ToDto()).ToList();
    }

    public async Task<DonationDto> CreateAsync(CreateDonationDto dto)
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.DonationsCreate))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية تسجيل تبرعات جديدة.");

        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId)) throw new UnauthorizedDomainException("لا يوجد حساب جمعية صالح.");
        if (string.IsNullOrWhiteSpace(dto.DonorName)) throw new ValidationDomainException("اسم المتبرع مطلوب.");
        if (dto.DonationKind is not ("نقدي" or "عيني" or "خدمة"))
            throw new ValidationDomainException("نوع التبرع غير معروف.");
        if (dto.DonationKind == "نقدي" && (!dto.Amount.HasValue || dto.Amount <= 0))
            throw new ValidationDomainException("قيمة التبرع النقدي يجب أن تكون أكبر من صفر.");
        if (dto.DonationKind == "عيني" && (!dto.Quantity.HasValue || !double.IsFinite(dto.Quantity.Value) || dto.Quantity <= 0))
            throw new ValidationDomainException("كمية التبرع العيني يجب أن تكون أكبر من صفر.");
        if (dto.EstimatedValue < 0) throw new ValidationDomainException("القيمة التقديرية لا يمكن أن تكون سالبة.");

        InventoryItem? item = null;
        if (dto.DonationKind is "عيني" or "خدمة")
        {
            if (string.IsNullOrWhiteSpace(dto.InventoryItemId)) throw new ValidationDomainException("اختيار الصنف أو الخدمة مطلوب.");
            item = await _inventoryRepo.GetByIdAsync(dto.InventoryItemId)
                ?? throw new EntityNotFoundException(nameof(InventoryItem), dto.InventoryItemId);
            if (item.CharityId != charityId) throw new UnauthorizedDomainException("لا يمكنك ربط تبرع بمخزن جمعية أخرى.");
            if ((dto.DonationKind == "عيني" && item.Kind != "صنف ملموس") || (dto.DonationKind == "خدمة" && item.Kind != "خدمة"))
                throw new ValidationDomainException("نوع التبرع لا يطابق الصنف المختار.");
        }
        var entity = dto.ToEntity(charityId);
        if (dto.DonationKind == "نقدي") entity.InventoryItemId = null;

        // If in-kind donation, auto increase active charity inventory
        if (item != null)
        {
            entity.ItemName = item.ItemName;
            entity.Unit = item.Unit;
            if (dto.DonationKind == "عيني")
            {
                _unitOfWork.ChangeInventoryBalance(item, dto.Quantity!.Value, allowNegativeBalance: true);
            }
        }

        await _donationRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }
}
