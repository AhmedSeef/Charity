using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Lookups;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/expense-categories")]
[Authorize]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IExpenseCategoryService _service;

    public ExpenseCategoriesController(IExpenseCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryDto>>> GetAll([FromQuery] string? month, [FromQuery] string? from, [FromQuery] string? to)
    {
        var categories = await _service.GetAllAsync(month, from, to);
        return Ok(categories);
    }

    [HttpGet("monthly-movements")]
    public async Task<ActionResult<IReadOnlyList<MonthlyExpenseMovementDto>>> GetMonthlyMovements([FromQuery] int? year, [FromQuery] string? from, [FromQuery] string? to)
    {
        var movements = await _service.GetMonthlyMovementsAsync(year, from, to);
        return Ok(movements);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseCategoryDto>> GetById(string id, [FromQuery] string? month, [FromQuery] string? from, [FromQuery] string? to)
    {
        var category = await _service.GetByIdAsync(id, month, from, to);
        return Ok(category);
    }

    [HttpGet("{id}/beneficiaries")]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryDisbursementDetailDto>>> GetDisbursementsByCategory(string id, [FromQuery] string? month, [FromQuery] string? from, [FromQuery] string? to)
    {
        var details = await _service.GetDisbursementsByCategoryAsync(id, month, from, to);
        return Ok(details);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryDto>> Create([FromBody] CreateExpenseCategoryDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseCategoryDto>> Update(string id, [FromBody] UpdateExpenseCategoryDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
