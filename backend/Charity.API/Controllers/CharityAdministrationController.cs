using Charity.Application.DTOs.Dashboard;
using Charity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/charities")]
[Authorize(Roles = "Admin")]
public sealed class CharityAdministrationController(ICharityProvisioningService provisioning) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CharityDto>> Create([FromBody] CreateCharityDto request)
    {
        var charity = await provisioning.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, charity);
    }

    [HttpPost("{charityId}/manager")]
    public async Task<IActionResult> CreateManager(string charityId, [FromBody] CreateCharityManagerDto request)
    {
        await provisioning.CreateManagerAsync(charityId, request);
        return StatusCode(StatusCodes.Status201Created);
    }
}
