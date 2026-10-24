using Charity.Application.DTOs.Dashboard;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Donation> _donationRepo;
    private readonly IRepository<InventoryItem> _inventoryRepo;
    private readonly IRepository<Distribution> _distributionRepo;
    private readonly IRepository<StaffMember> _staffRepo;
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly IRepository<Charity.Domain.Entities.Charity> _charityRepo;

    public DashboardService(
        IRepository<Donation> donationRepo,
        IRepository<InventoryItem> inventoryRepo,
        IRepository<Distribution> distributionRepo,
        IRepository<StaffMember> staffRepo,
        IRepository<Beneficiary> beneficiaryRepo,
        IRepository<Charity.Domain.Entities.Charity> charityRepo)
    {
        _donationRepo = donationRepo;
        _inventoryRepo = inventoryRepo;
        _distributionRepo = distributionRepo;
        _staffRepo = staffRepo;
        _beneficiaryRepo = beneficiaryRepo;
        _charityRepo = charityRepo;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var charityId = "network";
        var charityName = "شبكة الجمعيات بالكامل";

        // This endpoint is restricted to the system administrator and reports the whole network.
        var myDonations = await _donationRepo.GetAllAsync();
        var cashTotal = myDonations
            .Where(d => (d.DonationKind == "نقدي" || string.IsNullOrEmpty(d.DonationKind)) && d.Amount.HasValue)
            .Sum(d => d.Amount!.Value);
        var inKindCount = myDonations.Count(d => d.DonationKind == "عيني");

        var myInventory = await _inventoryRepo.GetAllAsync();
        var tangibleCount = myInventory.Count(i => i.Kind == "صنف ملموس");
        var servicesCount = myInventory.Count(i => i.Kind == "خدمة");

        var myDistributionsCount = await _distributionRepo.CountAsync();
        var myStaffCount = await _staffRepo.CountAsync();

        // Shared Federation Network stats
        var totalBeneficiaries = await _beneficiaryRepo.CountAsync();
        var totalCharities = await _charityRepo.CountAsync();
        var totalNetworkDistributions = await _distributionRepo.CountAsync();

        return new DashboardStatsDto
        {
            CharityId = charityId,
            CharityName = charityName,
            TotalCashDonations = cashTotal,
            InKindDonationsCount = inKindCount,
            TangibleItemsCount = tangibleCount,
            ServicesCount = servicesCount,
            DistributionsCount = myDistributionsCount,
            StaffCount = myStaffCount,
            TotalNetworkBeneficiariesCount = totalBeneficiaries,
            TotalPartnerCharitiesCount = totalCharities,
            TotalNetworkDistributionsCount = totalNetworkDistributions
        };
    }
}

public class CharityService : ICharityService
{
    private readonly IRepository<Charity.Domain.Entities.Charity> _charityRepo;
    private readonly IRepository<CharitySettings> _settingsRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CharityService(
        IRepository<Charity.Domain.Entities.Charity> charityRepo,
        IRepository<CharitySettings> settingsRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _charityRepo = charityRepo;
        _settingsRepo = settingsRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CharityDto>> GetAllCharitiesAsync()
    {
        var charities = _currentUserService.Role == "Admin"
            ? await _charityRepo.GetAllAsync()
            : await _charityRepo.FindAsync(c => c.Id == _currentUserService.CharityId);
        return charities.Select(c => c.ToDto()).ToList();
    }

    public async Task<CharitySettingsDto> GetMySettingsAsync()
    {
        LookupAccess.DemandManager(_currentUserService, allowAdmin: false);
        var charityId = TenantAccess.RequireCharity(_currentUserService);
        var settings = await _settingsRepo.FirstOrDefaultAsync(s => s.CharityId == charityId);
        if (settings != null)
        {
            return settings.ToDto();
        }

        var charity = await _charityRepo.GetByIdAsync(charityId);
        return new CharitySettingsDto
        {
            CharityId = charityId,
            CharityName = charity?.Name ?? "الجمعية",
            Logo = charity?.Logo ?? string.Empty,
            Phone = charity?.Phone ?? string.Empty,
            Details = charity?.RoleDesc ?? string.Empty
        };
    }

    public async Task<CharitySettingsDto> UpdateMySettingsAsync(CharitySettingsDto dto)
    {
        LookupAccess.DemandManager(_currentUserService, allowAdmin: false);
        var charityId = TenantAccess.RequireCharity(_currentUserService);
        var existing = await _settingsRepo.FirstOrDefaultAsync(s => s.CharityId == charityId);

        if (existing == null)
        {
            existing = new CharitySettings
            {
                CharityId = charityId,
                CharityName = dto.CharityName,
                Logo = dto.Logo,
                Phone = dto.Phone,
                Details = dto.Details
            };
            await _settingsRepo.AddAsync(existing);
        }
        else
        {
            existing.CharityName = dto.CharityName;
            existing.Logo = dto.Logo;
            existing.Phone = dto.Phone;
            existing.Details = dto.Details;
            await _settingsRepo.UpdateAsync(existing);
        }

        // Also update charity record name/logo
        var charity = await _charityRepo.GetByIdAsync(charityId);
        if (charity != null)
        {
            if (!string.IsNullOrEmpty(dto.CharityName)) charity.Name = dto.CharityName;
            if (!string.IsNullOrEmpty(dto.Logo)) charity.Logo = dto.Logo;
            if (!string.IsNullOrEmpty(dto.Phone)) charity.Phone = dto.Phone;
            await _charityRepo.UpdateAsync(charity);
        }

        await _unitOfWork.SaveChangesAsync();

        return existing.ToDto();
    }
}
