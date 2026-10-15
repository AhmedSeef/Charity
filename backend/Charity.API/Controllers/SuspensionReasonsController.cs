using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.SuspensionReasons;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/suspension-reasons")]
[Authorize]
public class SuspensionReasonsController : ControllerBase
{
    private readonly ISuspensionReasonService _service;

    public SuspensionReasonsController(ISuspensionReasonService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SuspensionReasonDto>>> GetAll()
    {
        var reasons = await _service.GetAllAsync();
        return Ok(reasons);
    }

    [HttpPost]
    public async Task<ActionResult<SuspensionReasonDto>> Create([FromBody] CreateSuspensionReasonDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
