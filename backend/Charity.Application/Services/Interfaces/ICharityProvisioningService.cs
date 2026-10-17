using Charity.Application.DTOs.Dashboard;

namespace Charity.Application.Services.Interfaces;

public interface ICharityProvisioningService
{
    Task<CharityDto> CreateAsync(CreateCharityDto request);
    Task CreateManagerAsync(string charityId, CreateCharityManagerDto request);
}
