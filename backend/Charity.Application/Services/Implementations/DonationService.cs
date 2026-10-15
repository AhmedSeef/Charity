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
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var donations = await _donationRepo.FindAsync(d => d.CharityId == charityId);
        return donations.OrderByDescending(d => d.Date).Select(d => d.ToDto()).ToList();
    }

    public async Task<DonationDto> CreateAsync(CreateDonationDto dto)
    {
        var charityId = _currentUserService.CharityId ?? "c1";
        var entity = dto.ToEntity(charityId);

        // If in-kind donation, auto increase active charity inventory
        if (dto.DonationKind == "عيني" && !string.IsNullOrEmpty(dto.InventoryItemId) && dto.Quantity.HasValue)
        {
            var item = await _inventoryRepo.GetByIdAsync(dto.InventoryItemId);
            if (item != null && item.CharityId == charityId && item.Kind != "خدمة")
            {
                item.Quantity += dto.Quantity.Value;
                entity.ItemName = item.ItemName;
                entity.Unit = item.Unit;
                await _inventoryRepo.UpdateAsync(item);
            }
        }

        await _donationRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }
}
