using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Dashboard;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class CharitiesController : ControllerBase
{
    private readonly ICharityService _charityService;

    public CharitiesController(ICharityService charityService)
    {
        _charityService = charityService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CharityDto>>> GetAll()
    {
        var result = await _charityService.GetAllCharitiesAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpGet("my-settings")]
    public async Task<ActionResult<CharitySettingsDto>> GetMySettings()
    {
        var result = await _charityService.GetMySettingsAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpPut("my-settings")]
    public async Task<ActionResult<CharitySettingsDto>> UpdateMySettings([FromBody] CharitySettingsDto dto)
    {
        var result = await _charityService.UpdateMySettingsAsync(dto);
        return Ok(result);
    }
}
