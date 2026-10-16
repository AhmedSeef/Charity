using Charity.Application.DTOs.Donations;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;

namespace Charity.UnitTests;

public class DonationServiceTests
{
    private readonly InventoryItem _item = new() { Id = "stock", CharityId = "c1", ItemName = "أرز", Unit = "كيلو", Quantity = 10 };
    private readonly FakeRepository<Donation> _donations = new();
    private readonly FakeRepository<InventoryItem> _inventory;
    private readonly FakeUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new();
    private readonly DonationService _service;

    public DonationServiceTests()
    {
        _inventory = new(_item); _service = new(_donations, _inventory, _user, _uow);
    }
    private static CreateDonationDto InKind(double? quantity = 3) => new() { DonorName = "متبرع", DonationKind = "عيني", InventoryItemId = "stock", Quantity = quantity };

    [Fact]
    public async Task InKindDonation_IncreasesStockAndUsesCatalogIdentity()
    {
        var request = InKind(); request.ItemName = "اسم مزيف"; request.Unit = "وحدة مزيفة";
        var result = await _service.CreateAsync(request);
        Assert.Equal(13, _item.Quantity); Assert.Equal("أرز", result.ItemName); Assert.Equal("كيلو", result.Unit);
        Assert.Single(_donations.Items); Assert.Equal(1, _uow.SaveCalls);
    }

    [Fact]
    public async Task CashDonation_DoesNotTouchStock()
    {
        var result = await _service.CreateAsync(new() { DonorName = "متبرع", Amount = 100 });
        Assert.Equal(100m, result.Amount); Assert.Equal(10, _item.Quantity); Assert.Equal(0, _inventory.UpdateCalls);
    }

    [Fact]
    public async Task ServiceDonation_DoesNotTouchStockAndUsesCatalogIdentity()
    {
        _item.Kind = "خدمة";
        var request = InKind(); request.DonationKind = "خدمة";
        var result = await _service.CreateAsync(request);
        Assert.Equal("أرز", result.ItemName); Assert.Equal(10, _item.Quantity); Assert.Equal(0, _inventory.UpdateCalls);
    }

    [Theory]
    [InlineData(null)] [InlineData(0)] [InlineData(-1)]
    public async Task InvalidCashAmount_IsRejected(int? amount)
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(new() { DonorName = "متبرع", Amount = amount }));
        AssertNoWrite();
    }

    [Theory]
    [InlineData(null)] [InlineData(0d)] [InlineData(-2d)] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)]
    public async Task InvalidInKindQuantity_IsRejected(double? quantity)
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(InKind(quantity)));
        AssertNoWrite();
    }

    [Fact]
    public async Task ForeignInventory_IsRejected()
    {
        _item.CharityId = "c2";
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => _service.CreateAsync(InKind()));
        AssertNoWrite();
    }

    [Fact]
    public async Task MissingInventory_IsRejected()
    {
        var request = InKind(); request.InventoryItemId = "missing";
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateAsync(request));
        AssertNoWrite();
    }

    [Fact]
    public async Task InKindWithoutInventoryReference_IsRejected()
    {
        var request = InKind(); request.InventoryItemId = null;
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(request));
        AssertNoWrite();
    }

    [Theory]
    [InlineData("عيني", "خدمة")] [InlineData("خدمة", "صنف ملموس")]
    public async Task DonationKindMustMatchInventoryKind(string donationKind, string inventoryKind)
    {
        _item.Kind = inventoryKind;
        var request = InKind(); request.DonationKind = donationKind;
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(request));
        AssertNoWrite();
    }

    [Fact]
    public async Task UnknownDonationKind_IsRejected()
    {
        var request = InKind(); request.DonationKind = "unsupported";
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateAsync(request));
        AssertNoWrite();
    }

    [Fact]
    public async Task MissingTenantClaim_CannotFallBackToC1()
    {
        _user.CharityId = null;
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => _service.CreateAsync(InKind()));
        AssertNoWrite();
    }

    [Fact]
    public async Task DonationList_OnlyContainsCurrentCharitysDonations()
    {
        _donations.Items.AddRange([new() { CharityId = "c1" }, new() { CharityId = "c2" }]);
        Assert.Equal("c1", Assert.Single(await _service.GetMyDonationsAsync()).CharityId);
    }

    private void AssertNoWrite()
    {
        Assert.Equal(10, _item.Quantity); Assert.Empty(_donations.Items);
        Assert.Equal(0, _inventory.UpdateCalls); Assert.Equal(0, _uow.SaveCalls);
    }
}
