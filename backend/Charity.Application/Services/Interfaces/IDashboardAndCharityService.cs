using Charity.Application.DTOs.Dashboard;

namespace Charity.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public interface ICharityService
{
    Task<IReadOnlyList<CharityDto>> GetAllCharitiesAsync();
    Task<CharitySettingsDto> GetMySettingsAsync();
    Task<CharitySettingsDto> UpdateMySettingsAsync(CharitySettingsDto dto);
}
