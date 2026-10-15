using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Inventory;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetAll()
    {
        var result = await _inventoryService.GetMyInventoryAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryItemDto>> GetById(string id)
    {
        var result = await _inventoryService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "الصنف غير موجود" });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> Create([FromBody] CreateInventoryItemDto dto)
    {
        var result = await _inventoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InventoryItemDto>> Update(string id, [FromBody] UpdateInventoryItemDto dto)
    {
        var result = await _inventoryService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpPost("{id}/adjust-stock")]
    public async Task<IActionResult> AdjustStock(string id, [FromBody] AdjustStockDto dto)
    {
        await _inventoryService.AdjustStockAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _inventoryService.DeleteAsync(id);
        return NoContent();
    }
}
