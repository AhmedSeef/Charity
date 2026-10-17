using Charity.Application.DTOs.SeparationReasons;

namespace Charity.Application.Services.Interfaces;

public interface ISeparationReasonService
{
    Task<IReadOnlyList<SeparationReasonDto>> GetAllAsync();
    Task<SeparationReasonDto> CreateAsync(CreateSeparationReasonDto dto);
    Task DeleteAsync(string id);
}
