using Charity.Application.DTOs.Inventory;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;

namespace Charity.UnitTests;

public class InventoryServiceTests
{
    private readonly InventoryItem _item = new() { Id = "stock", CharityId = "c1", ItemName = "أرز", Quantity = 10 };
    private readonly FakeRepository<InventoryItem> _inventory;
    private readonly FakeUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new();
    private readonly InventoryService _service;

    public InventoryServiceTests() { _inventory = new(_item); _service = new(_inventory, _user, _uow); }

    [Fact]
    public async Task CreateService_AlwaysStartsWithZeroStock()
    {
        var result = await _service.CreateAsync(new() { ItemName = "كشف طبي", Kind = "خدمة", Quantity = 20 });
        Assert.Equal(0, result.Quantity); Assert.Equal("c1", result.CharityId);
    }

    [Theory]
    [InlineData(-1)] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)]
    public async Task CreateWithInvalidStock_IsRejected(double quantity)
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(new() { ItemName = "أرز", Quantity = quantity }));
        Assert.Equal(0, _inventory.AddCalls); Assert.Equal(0, _uow.SaveCalls);
    }

    [Fact]
    public async Task UpdateWithNegativeStock_IsRejectedWithoutMutation()
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.UpdateAsync("stock", new() { ItemName = "أرز", Quantity = -1 }));
        Assert.Equal(10, _item.Quantity); Assert.Equal(0, _uow.SaveCalls);
    }

    [Theory]
    [InlineData(-1)] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)]
    public async Task StockAdjustmentMustBeFiniteAndNonnegative(double quantity)
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.AdjustStockAsync("stock", new() { NewQuantity = quantity }));
        Assert.Equal(10, _item.Quantity); Assert.Equal(0, _uow.SaveCalls);
    }

    [Fact]
    public async Task ZeroStockAdjustment_IsValid()
    {
        await _service.AdjustStockAsync("stock", new() { NewQuantity = 0 });
        Assert.Equal(0, _item.Quantity); Assert.Equal(1, _uow.SaveCalls);
    }

    [Fact]
    public async Task ServiceCannotAcquireWarehouseStockViaAdjustment()
    {
        _item.Kind = "خدمة"; _item.Quantity = 0;
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.AdjustStockAsync("stock", new() { NewQuantity = 10 }));
        Assert.Equal(0, _item.Quantity); Assert.Equal(0, _uow.SaveCalls);
    }

    [Theory]
    [InlineData("update")] [InlineData("adjust")] [InlineData("delete")]
    public async Task ForeignInventoryCannotBeMutated(string action)
    {
        _item.CharityId = "c2";
        await Assert.ThrowsAsync<UnauthorizedDomainException>(async () =>
        {
            if (action == "update") await _service.UpdateAsync("stock", new() { ItemName = "أرز", Quantity = 1 });
            if (action == "adjust") await _service.AdjustStockAsync("stock", new() { NewQuantity = 1 });
            if (action == "delete") await _service.DeleteAsync("stock");
        });
        Assert.Equal(10, _item.Quantity); Assert.False(_item.IsDeleted); Assert.Equal(0, _uow.SaveCalls);
    }

    [Fact]
    public async Task ForeignInventoryIsHiddenFromDetailAndList()
    {
        _item.CharityId = "c2";
        Assert.Null(await _service.GetByIdAsync("stock")); Assert.Empty(await _service.GetMyInventoryAsync());
    }

    [Fact]
    public async Task MissingTenantClaim_CannotCreateStockForC1()
    {
        _user.CharityId = null;
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => _service.CreateAsync(new() { ItemName = "أرز" }));
        Assert.Equal(0, _inventory.AddCalls); Assert.Equal(0, _uow.SaveCalls);
    }

    [Fact]
    public async Task NegativeAlertThreshold_IsRejected()
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(new() { ItemName = "أرز", MinAlert = -1 }));
        Assert.Equal(0, _inventory.AddCalls); Assert.Equal(0, _uow.SaveCalls);
    }

    [Fact]
    public async Task CatalogMetadataUpdateWithoutQuantityPreservesStock()
    {
        var updated = await _service.UpdateAsync("stock", new() { ItemName = "أرز معبأ", Kind = "صنف ملموس", MinAlert = 5 });
        Assert.Equal(10, updated.Quantity);
        Assert.Equal("أرز معبأ", updated.ItemName);
    }

    [Fact]
    public async Task CatalogMetadataUpdatePreservesAnOverdrawnBalance()
    {
        _item.Quantity = -3;
        var updated = await _service.UpdateAsync("stock", new() { ItemName = "أرز معبأ", Kind = "صنف ملموس", MinAlert = 5 });
        Assert.Equal(-3, updated.Quantity);
        Assert.Equal("أرز معبأ", updated.ItemName);
    }
}
