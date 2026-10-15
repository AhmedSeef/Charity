using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Analytics;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IBiAnalyticsService _service;

    public AnalyticsController(IBiAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet("bi")]
    public async Task<ActionResult<BiAnalyticsDto>> GetBiAnalytics([FromQuery] string? charityId = null)
    {
        var data = await _service.GetCharityAnalyticsAsync(charityId);
        return Ok(data);
    }
}
