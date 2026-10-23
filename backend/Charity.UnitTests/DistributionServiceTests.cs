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
    private readonly FakeRepository<BeneficiaryFamilyMember> _familyMembers = new();
    private readonly FakeRepository<ExpenseCategory> _expenseCategories = new();
    private readonly FakeRepository<Donation> _donations = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new();
    private readonly DistributionService _service;

    public DistributionServiceTests()
    {
        _inventory = new(_item); _beneficiaries = new(_beneficiary);
        _service = new(_distributions, _inventory, _beneficiaries, _familyMembers, _expenseCategories, _user, _uow, _donations);
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
        await Assert.ThrowsAsync<BalanceConfirmationRequiredException>(() => _service.CreateDistributionAsync(Request(11)));
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
    public async Task DistributionEndpoints_AreTenantScoped_SharedHistoryBelongsToBeneficiaryProfile()
    {
        _distributions.Items.AddRange([
            new() { BeneficiaryId = "beneficiary", CharityId = "c1", Date = "2026-10-01" },
            new() { BeneficiaryId = "beneficiary", CharityId = "c2", Date = "2026-10-02" },
            new() { BeneficiaryId = "other", CharityId = "c2" }
        ]);
        Assert.Single(await _service.GetMyCharityDistributionsAsync());
        var summary = await _service.GetBeneficiaryAidSummaryAsync("beneficiary");
        Assert.Equal(1, summary.TotalDistributionsCount);
        Assert.Equal(0, summary.OtherCharitiesDistributionsCount);
        Assert.True(summary.AidHistory[0].IsFromCurrentCharity);
    }

    [Fact]
    public async Task CashAid_WithoutExpenseCategory_ThrowsValidationDomainException()
    {
        var dto = new CreateDistributionDto
        {
            BeneficiaryId = "beneficiary",
            Kind = "إعانة نقدية",
            Amount = 1500,
            Quantity = 1,
            ExpenseCategoryId = null
        };

        var ex = await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateDistributionAsync(dto));
        Assert.Contains("يجب اختيار بند ونوع المصروف", ex.Message);
    }

    [Fact]
    public async Task CashAid_WithExpenseCategory_AssignsCategoryAndAmountSuccessfully()
    {
        var cat = new ExpenseCategory { Id = "exp_monthly", CharityId = "c1", Name = "مساعدات نقدية شهرية", Code = "EXP-002" };
        await _expenseCategories.AddAsync(cat);
        _donations.Items.Add(new Donation { CharityId = "c1", DonationKind = "نقدي", Amount = 1500 });

        var dto = new CreateDistributionDto
        {
            BeneficiaryId = "beneficiary",
            Kind = "إعانة نقدية",
            Amount = 1500,
            Quantity = 1,
            ExpenseCategoryId = "exp_monthly"
        };

        var result = await _service.CreateDistributionAsync(dto);
        Assert.Equal("إعانة نقدية", result.Kind);
        Assert.Equal(1500, result.Amount);
        Assert.Equal("exp_monthly", result.ExpenseCategoryId);
        Assert.Equal("مساعدات نقدية شهرية", result.ExpenseCategoryName);
        Assert.Equal("مساعدات نقدية شهرية", result.ItemName);
    }

    [Fact]
    public async Task BulkDistribution_WithTangibleItem_DeductsStockAccuratelyAndCreatesDistributions()
    {
        var ben2 = new Beneficiary { Id = "ben2", Name = "مستفيد 2", Status = "مستحق فعال" };
        var ben3 = new Beneficiary { Id = "ben3", Name = "مستفيد 3", Status = "مستحق فعال" };
        await _beneficiaries.AddAsync(ben2);
        await _beneficiaries.AddAsync(ben3);

        var bulkDto = new BulkDistributionRequestDto
        {
            BeneficiaryIds = new List<string> { "beneficiary", "ben2", "ben3" },
            InventoryItemId = "stock",
            QuantityPerBeneficiary = 2,
            VolunteerOrWorkerName = "أحمد المتطوع",
            TargetAreaName = "منطقة المنشر"
        };

        var result = await _service.CreateBulkDistributionAsync(bulkDto);

        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(6, result.TotalQuantityDisbursed);
        Assert.Equal(4, _item.Quantity); // 10 - 6 = 4
        Assert.Equal(3, _distributions.Items.Count);
        Assert.Equal("أحمد المتطوع", result.VolunteerOrWorkerName);
        Assert.Equal("منطقة المنشر", result.TargetAreaName);
    }

    [Fact]
    public async Task BulkDistribution_WhenStockInsufficient_RequiresConfirmation()
    {
        var ben2 = new Beneficiary { Id = "ben2", Name = "مستفيد 2", Status = "مستحق فعال" };
        await _beneficiaries.AddAsync(ben2);

        var bulkDto = new BulkDistributionRequestDto
        {
            BeneficiaryIds = new List<string> { "beneficiary", "ben2" },
            InventoryItemId = "stock",
            QuantityPerBeneficiary = 6 // 2 * 6 = 12 > 10 in stock
        };

        var ex = await Assert.ThrowsAsync<BalanceConfirmationRequiredException>(() => _service.CreateBulkDistributionAsync(bulkDto));
        Assert.Contains("هل تريد الاستمرار", ex.Message);
        Assert.Equal(10, _item.Quantity);
    }

    [Fact]
    public async Task BulkDistribution_RejectsFamiliesOutsideRequestedAreaBeforeWriting()
    {
        _beneficiary.VillageAreaName = "المنشر";
        await _beneficiaries.AddAsync(new Beneficiary { Id = "outside", Name = "خارج المنطقة", VillageAreaName = "السوق" });

        var request = new BulkDistributionRequestDto
        {
            BeneficiaryIds = ["beneficiary", "outside"],
            TargetAreaFilter = "المنشر",
            InventoryItemId = "stock",
            QuantityPerBeneficiary = 1
        };

        var ex = await Assert.ThrowsAsync<ValidationDomainException>(() => _service.CreateBulkDistributionAsync(request));
        Assert.Contains("خارج منطقة", ex.Message);
        AssertNoWrite();

        request.BeneficiaryIds = ["beneficiary"];
        var result = await _service.CreateBulkDistributionAsync(request);
        Assert.Equal(1, result.SuccessCount);
        Assert.Single(result.CreatedDistributions);
    }

    [Fact]
    public async Task BulkDistribution_SkipsSuspendedBeneficiaryAndContinuesForActive()
    {
        var activeBen = new Beneficiary { Id = "active_ben", Name = "مستفيد نشط", Status = "مستحق فعال" };
        var suspendedBen = new Beneficiary { Id = "susp_ben", Name = "مستفيد موقوف", IsSuspended = true, Status = "موقوف", SuspensionReasonTitle = "تحسن الدخل" };
        await _beneficiaries.AddAsync(activeBen);
        await _beneficiaries.AddAsync(suspendedBen);

        var bulkDto = new BulkDistributionRequestDto
        {
            BeneficiaryIds = new List<string> { "active_ben", "susp_ben" },
            InventoryItemId = "stock",
            QuantityPerBeneficiary = 1
        };

        var result = await _service.CreateBulkDistributionAsync(bulkDto);

        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Single(result.SkippedBeneficiaryNames);
        Assert.Contains("مستفيد موقوف", result.SkippedBeneficiaryNames[0]);
        Assert.Equal(9, _item.Quantity); // 10 - 1 = 9
    }

    private void AssertNoWrite()
    {
        Assert.Equal(10, _item.Quantity);
        Assert.Empty(_distributions.Items);
        Assert.Equal(0, _inventory.UpdateCalls);
        Assert.Equal(0, _uow.SaveCalls);
    }
}
