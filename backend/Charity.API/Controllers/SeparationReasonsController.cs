using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.SeparationReasons;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/separation-reasons")]
[Authorize]
public class SeparationReasonsController : ControllerBase
{
    private readonly ISeparationReasonService _service;

    public SeparationReasonsController(ISeparationReasonService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SeparationReasonDto>>> GetAll()
    {
        var reasons = await _service.GetAllAsync();
        return Ok(reasons);
    }

    [HttpPost]
    public async Task<ActionResult<SeparationReasonDto>> Create([FromBody] CreateSeparationReasonDto dto)
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
