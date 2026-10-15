using Charity.Application.DTOs.SuspensionReasons;

namespace Charity.Application.Services.Interfaces;

public interface ISuspensionReasonService
{
    Task<IReadOnlyList<SuspensionReasonDto>> GetAllAsync();
    Task<SuspensionReasonDto> CreateAsync(CreateSuspensionReasonDto dto);
    Task DeleteAsync(string id);
}
