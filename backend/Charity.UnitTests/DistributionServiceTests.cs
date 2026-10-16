using Charity.Application.DTOs.Distributions;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;

namespace Charity.UnitTests;

public class DistributionServiceTests
{
    private readonly InventoryItem _item = new() { Id = "stock", CharityId = "c1", ItemName = "أرز", Unit = "كيلو", Quantity = 10 };
    private readonly Beneficiary _beneficiary = new() { Id = "beneficiary", Name = "مستفيد", RegisteredByCharityId = "c2" };
    private readonly FakeRepository<Distribution> _distributions = new();
    private readonly FakeRepository<InventoryItem> _inventory;
    private readonly FakeRepository<Beneficiary> _beneficiaries;
    private readonly FakeUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new();
    private readonly DistributionService _service;

    public DistributionServiceTests()
    {
        _inventory = new(_item); _beneficiaries = new(_beneficiary);
        _service = new(_distributions, _inventory, _beneficiaries, _user, _uow);
    }

    private static CreateDistributionDto Request(double quantity = 3) => new() { BeneficiaryId = "beneficiary", InventoryItemId = "stock", Quantity = quantity };

    [Fact]
    public async Task TangibleAid_DeductsStockAndAllowsAnotherCharitysBeneficiary()
    {
        var result = await _service.CreateDistributionAsync(Request());
        Assert.Equal(7, _item.Quantity);
        Assert.Equal("c1", result.CharityId);
        Assert.Equal("أرز", result.ItemName);
        Assert.Equal("كيلو", result.Unit);
        Assert.Single(_distributions.Items);
        Assert.Equal(1, _uow.SaveCalls);
    }

    [Fact]
    public async Task ExactAvailableQuantity_LeavesZeroStock()
    {
        await _service.CreateDistributionAsync(Request(10));
        Assert.Equal(0, _item.Quantity);
    }

    [Fact]
    public async Task ServiceAid_DoesNotDeductStock()
    {
        _item.Kind = "خدمة"; _item.Quantity = 0;
        var result = await _service.CreateDistributionAsync(Request(20));
        Assert.Equal("خدمة", result.Kind);
        Assert.Equal(0, _item.Quantity);
        Assert.Equal(0, _inventory.UpdateCalls);
    }

    [Theory]
    [InlineData(-1)] [InlineData(0)] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)]
    public async Task InvalidQuantity_IsRejectedWithoutAnyWrite(double quantity)
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateDistributionAsync(Request(quantity)));
        AssertNoWrite();
    }

    [Fact]
    public async Task InsufficientStock_IsRejectedWithoutAnyWrite()
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateDistributionAsync(Request(11)));
        AssertNoWrite();
    }

    [Theory]
    [InlineData(true, "مستحق فعال")] [InlineData(false, "موقوف")]
    public async Task SuspendedBeneficiary_IsRejectedWithoutAnyWrite(bool suspended, string status)
    {
        _beneficiary.IsSuspended = suspended; _beneficiary.Status = status;
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateDistributionAsync(Request()));
        AssertNoWrite();
    }

    [Fact]
    public async Task ForeignInventory_IsRejectedWithoutAnyWrite()
    {
        _item.CharityId = "c2";
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => _service.CreateDistributionAsync(Request()));
        AssertNoWrite();
    }

    [Fact]
    public async Task MissingInventory_IsRejectedWithoutAnyWrite()
    {
        var request = Request(); request.InventoryItemId = "missing";
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateDistributionAsync(request));
        AssertNoWrite();
    }

    [Fact]
    public async Task TangibleAidWithoutInventory_IsRejectedWithoutAnyWrite()
    {
        var request = Request(); request.InventoryItemId = null;
        await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateDistributionAsync(request));
        AssertNoWrite();
    }

    [Fact]
    public async Task MissingBeneficiary_IsRejectedWithoutAnyWrite()
    {
        var request = Request(); request.BeneficiaryId = "missing";
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateDistributionAsync(request));
        AssertNoWrite();
    }

    [Theory]
    [InlineData(null)] [InlineData("")]
    public async Task MissingTenantClaim_CannotFallBackToC1(string? charityId)
    {
        _user.CharityId = charityId;
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => _service.CreateDistributionAsync(Request()));
        AssertNoWrite();
    }

    [Fact]
    public async Task OwnList_IsIsolatedWhileAidHistoryIncludesPartnerCharities()
    {
        _distributions.Items.AddRange([
            new() { BeneficiaryId = "beneficiary", CharityId = "c1", Date = "2026-10-01" },
            new() { BeneficiaryId = "beneficiary", CharityId = "c2", Date = "2026-10-02" },
            new() { BeneficiaryId = "other", CharityId = "c2" }
        ]);
        Assert.Single(await _service.GetMyCharityDistributionsAsync());
        var summary = await _service.GetBeneficiaryAidSummaryAsync("beneficiary");
        Assert.Equal(2, summary.TotalDistributionsCount);
        Assert.Equal(1, summary.OtherCharitiesDistributionsCount);
        Assert.False(summary.AidHistory[0].IsFromCurrentCharity);
    }

    private void AssertNoWrite()
    {
        Assert.Equal(10, _item.Quantity);
        Assert.Empty(_distributions.Items);
        Assert.Equal(0, _inventory.UpdateCalls);
        Assert.Equal(0, _uow.SaveCalls);
    }
}
