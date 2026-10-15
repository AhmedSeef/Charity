using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Staff;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffMemberDto>>> GetAll()
    {
        var result = await _staffService.GetMyStaffAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StaffMemberDto>> GetById(string id)
    {
        var result = await _staffService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "عضو فريق العمل غير موجود" });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<StaffMemberDto>> Create([FromBody] CreateStaffMemberDto dto)
    {
        var result = await _staffService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StaffMemberDto>> Update(string id, [FromBody] UpdateStaffMemberDto dto)
    {
        var result = await _staffService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _staffService.DeleteAsync(id);
        return NoContent();
    }
}
