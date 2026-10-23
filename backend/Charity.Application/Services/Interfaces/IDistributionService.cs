using Charity.Application.DTOs.Distributions;

namespace Charity.Application.Services.Interfaces;

public interface IDistributionService
{
    Task<IReadOnlyList<DistributionDto>> GetMyCharityDistributionsAsync();
    Task<IReadOnlyList<DistributionDto>> GetAllNetworkDistributionsAsync();
    Task<IReadOnlyList<DistributionDto>> GetDistributionsByBeneficiaryIdAsync(string beneficiaryId);
    Task<BeneficiaryAidSummaryDto> GetBeneficiaryAidSummaryAsync(string beneficiaryId);
    Task<DistributionDto> CreateDistributionAsync(CreateDistributionDto dto);
    Task<BulkDistributionResultDto> CreateBulkDistributionAsync(BulkDistributionRequestDto dto);
}
