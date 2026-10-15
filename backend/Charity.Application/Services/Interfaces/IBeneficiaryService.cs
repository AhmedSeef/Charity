using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;

namespace Charity.Application.Services.Interfaces;

public interface IBeneficiaryService
{
    Task<IReadOnlyList<BeneficiaryDto>> GetAllAsync();
    Task<BeneficiaryDto?> GetByIdAsync(string id);
    Task<BeneficiaryDto> CreateAsync(CreateBeneficiaryDto dto);
    Task<BeneficiaryDto> UpdateAsync(string id, UpdateBeneficiaryDto dto);
    Task<BeneficiaryAidSummaryDto> GetAidSummaryAsync(string beneficiaryId);
    Task<BeneficiaryDto> SuspendAsync(string id, SuspendBeneficiaryDto dto);
    Task<BeneficiaryDto> ReactivateAsync(string id);
    Task<BeneficiaryDto> SeparateFamilyMemberAsync(string beneficiaryId, SeparateFamilyMemberDto dto);
}
