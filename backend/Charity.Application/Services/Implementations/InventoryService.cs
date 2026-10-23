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
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryView) &&
            !_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryManage))
        {
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية استعراض المخازن والأصناف.");
        }

        var charityId = _currentUserService.CharityId ?? string.Empty;
        var items = await _inventoryRepo.FindAsync(i => i.CharityId == charityId);
        return items.Select(i => i.ToDto()).ToList();
    }

    public async Task<InventoryItemDto?> GetByIdAsync(string id)
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryView) &&
            !_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryManage))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية استعراض المخازن والأصناف.");
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var item = await _inventoryRepo.GetByIdAsync(id);
        if (item == null || item.CharityId != charityId) return null;
        return item.ToDto();
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto)
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryManage))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية إضافة وتعديل الأصناف والمخزون.");

        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId)) throw new UnauthorizedDomainException("لا يوجد حساب جمعية صالح.");
        ValidateStock(dto.Quantity, dto.MinAlert);
        var entity = dto.ToEntity(charityId);

        await _inventoryRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<InventoryItemDto> UpdateAsync(string id, UpdateInventoryItemDto dto)
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryManage))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية تعديل الأصناف والمخزون.");

        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _inventoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(InventoryItem), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك تعديل صنف مخزني لجمعية أخرى!");
        }

        // Metadata-only edits must preserve a negative balance created by confirmed disbursements.
        ValidateStock(dto.Quantity ?? 0, dto.MinAlert);

        existing.Code = dto.Code;
        existing.ItemName = dto.ItemName;
        existing.Kind = dto.Kind;
        existing.Unit = dto.Unit;
        existing.Type = dto.Type;
        existing.Quantity = dto.Kind == "خدمة" ? 0 : dto.Quantity ?? existing.Quantity;
        existing.MinAlert = dto.MinAlert;
        existing.Notes = dto.Notes;

        await _inventoryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToDto();
    }

    public async Task AdjustStockAsync(string id, AdjustStockDto dto)
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryManage))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية جرد وتعديل أرصدة المخزون.");

        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _inventoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(InventoryItem), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك جرد مخزن جمعية أخرى!");
        }

        ValidateStock(dto.NewQuantity, existing.MinAlert);
        if (existing.Kind == "خدمة" && dto.NewQuantity != 0)
            throw new ValidationDomainException("الخدمات لا تملك رصيداً مخزنياً.");

        existing.Quantity = dto.NewQuantity;
        await _inventoryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        if (!_currentUserService.HasPermission(Charity.Domain.Security.AppPermissions.InventoryManage))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية حذف الأصناف.");

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

    private static void ValidateStock(double quantity, double minAlert)
    {
        if (!double.IsFinite(quantity) || quantity < 0)
            throw new ValidationDomainException("رصيد المخزن يجب أن يكون عدداً غير سالب.");
        if (!double.IsFinite(minAlert) || minAlert < 0)
            throw new ValidationDomainException("حد التنبيه يجب أن يكون عدداً غير سالب.");
    }
}
