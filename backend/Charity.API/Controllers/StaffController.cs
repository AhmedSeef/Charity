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

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<StaffUserDto>>> GetStaffUsers()
    {
        var result = await _staffService.GetStaffUsersAsync();
        return Ok(result);
    }

    [HttpPost("users")]
    public async Task<ActionResult<StaffUserDto>> CreateStaffUser([FromBody] CreateStaffUserDto dto)
    {
        var result = await _staffService.CreateStaffUserAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("users/{userId}/permissions")]
    public async Task<ActionResult<StaffUserDto>> UpdateStaffUserPermissions(string userId, [FromBody] UpdateStaffUserPermissionsDto dto)
    {
        var result = await _staffService.UpdateStaffUserPermissionsAsync(userId, dto);
        return Ok(result);
    }

    [HttpPut("users/{userId}/password")]
    public async Task<IActionResult> ResetStaffUserPassword(string userId, [FromBody] ChangeStaffUserPasswordDto dto)
    {
        await _staffService.ResetStaffUserPasswordAsync(userId, dto);
        return NoContent();
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteStaffUser(string userId)
    {
        await _staffService.DeleteStaffUserAsync(userId);
        return NoContent();
    }
}
