using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Distributions;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DistributionsController : ControllerBase
{
    private readonly IDistributionService _distributionService;

    public DistributionsController(IDistributionService distributionService)
    {
        _distributionService = distributionService;
    }

    [HttpGet("my-charity")]
    public async Task<ActionResult<IReadOnlyList<DistributionDto>>> GetMyCharityDistributions()
    {
        var result = await _distributionService.GetMyCharityDistributionsAsync();
        return Ok(result);
    }

    [HttpGet("all-network")]
    public async Task<ActionResult<IReadOnlyList<DistributionDto>>> GetAllNetworkDistributions()
    {
        var result = await _distributionService.GetAllNetworkDistributionsAsync();
        return Ok(result);
    }

    [HttpGet("beneficiary/{beneficiaryId}")]
    public async Task<ActionResult<IReadOnlyList<DistributionDto>>> GetByBeneficiary(string beneficiaryId)
    {
        var result = await _distributionService.GetDistributionsByBeneficiaryIdAsync(beneficiaryId);
        return Ok(result);
    }

    [HttpGet("beneficiary/{beneficiaryId}/summary")]
    public async Task<ActionResult<BeneficiaryAidSummaryDto>> GetBeneficiarySummary(string beneficiaryId)
    {
        var result = await _distributionService.GetBeneficiaryAidSummaryAsync(beneficiaryId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DistributionDto>> Create([FromBody] CreateDistributionDto dto)
    {
        var result = await _distributionService.CreateDistributionAsync(dto);
        return Ok(result);
    }
}
