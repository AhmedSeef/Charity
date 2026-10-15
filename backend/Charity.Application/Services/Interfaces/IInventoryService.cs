using Charity.Application.DTOs.Inventory;

namespace Charity.Application.Services.Interfaces;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItemDto>> GetMyInventoryAsync();
    Task<InventoryItemDto?> GetByIdAsync(string id);
    Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto);
    Task<InventoryItemDto> UpdateAsync(string id, UpdateInventoryItemDto dto);
    Task AdjustStockAsync(string id, AdjustStockDto dto);
    Task DeleteAsync(string id);
}
