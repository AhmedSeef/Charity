using Charity.Application.DTOs.Inventory;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IRepository<InventoryItem> _inventoryRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IRepository<InventoryItem> inventoryRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _inventoryRepo = inventoryRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetMyInventoryAsync()
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var items = await _inventoryRepo.FindAsync(i => i.CharityId == charityId);
        return items.Select(i => i.ToDto()).ToList();
    }

    public async Task<InventoryItemDto?> GetByIdAsync(string id)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var item = await _inventoryRepo.GetByIdAsync(id);
        if (item == null || item.CharityId != charityId) return null;
        return item.ToDto();
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto)
    {
        var charityId = _currentUserService.CharityId ?? "c1";
        var entity = dto.ToEntity(charityId);

        await _inventoryRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<InventoryItemDto> UpdateAsync(string id, UpdateInventoryItemDto dto)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _inventoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(InventoryItem), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك تعديل صنف مخزني لجمعية أخرى!");
        }

        existing.Code = dto.Code;
        existing.ItemName = dto.ItemName;
        existing.Kind = dto.Kind;
        existing.Unit = dto.Unit;
        existing.Type = dto.Type;
        existing.Quantity = dto.Kind == "خدمة" ? 0 : dto.Quantity;
        existing.MinAlert = dto.MinAlert;
        existing.Notes = dto.Notes;

        await _inventoryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToDto();
    }

    public async Task AdjustStockAsync(string id, AdjustStockDto dto)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _inventoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(InventoryItem), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك جرد مخزن جمعية أخرى!");
        }

        existing.Quantity = dto.NewQuantity;
        await _inventoryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _inventoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(InventoryItem), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك حذف صنف لجمعية أخرى!");
        }

        await _inventoryRepo.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
