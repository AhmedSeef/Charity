using Charity.Application.DTOs.Donations;

namespace Charity.Application.Services.Interfaces;

public interface IDonationService
{
    Task<IReadOnlyList<DonationDto>> GetMyDonationsAsync();
    Task<DonationDto> CreateAsync(CreateDonationDto dto);
}
