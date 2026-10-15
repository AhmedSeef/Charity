using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Donations;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;

    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DonationDto>>> GetAll()
    {
        var result = await _donationService.GetMyDonationsAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DonationDto>> Create([FromBody] CreateDonationDto dto)
    {
        var result = await _donationService.CreateAsync(dto);
        return Ok(result);
    }
}
