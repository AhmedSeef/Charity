using Charity.Application.DTOs.Analytics;

namespace Charity.Application.Services.Interfaces;

public interface IBiAnalyticsService
{
    Task<BiAnalyticsDto> GetCharityAnalyticsAsync(string? charityId = null);
}
