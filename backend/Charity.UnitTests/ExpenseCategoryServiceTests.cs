using Charity.Application.DTOs.Lookups;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Xunit;

namespace Charity.UnitTests;

public class ExpenseCategoryServiceTests
{
    private readonly FakeRepository<ExpenseCategory> _categoryRepo = new();
    private readonly FakeRepository<Distribution> _distributionRepo = new();
    private readonly FakeRepository<Beneficiary> _beneficiaryRepo = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly FakeUnitOfWork _uow = new();

    private ExpenseCategoryService CreateService()
    {
        return new ExpenseCategoryService(_categoryRepo, _distributionRepo, _beneficiaryRepo, _currentUser, _uow);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_AddsExpenseCategoryWithGeneratedCode()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateExpenseCategoryDto
        {
            Name = "مساعدات زواج وتجهيز عرائس",
            Description = "مساعدات للفتيات اليتيمات والمقبلات على الزواج"
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("مساعدات زواج وتجهيز عرائس", result.Name);
        Assert.Equal("EXP-001", result.Code);
        Assert.True(result.IsActive);
        Assert.Equal(1, _uow.SaveCalls);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsValidationDomainException()
    {
        // Arrange
        var service = CreateService();
        var existing = new ExpenseCategory { Id = "c1", CharityId = "c1", Name = "مساعدات علاجية", Code = "EXP-001" };
        await _categoryRepo.AddAsync(existing);

        var dto = new CreateExpenseCategoryDto
        {
            Name = "  مساعدات علاجية  "
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationDomainException>(() => service.CreateAsync(dto));
        Assert.Contains("مسجل مسبقاً", ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_CalculatesTotalsAndCountsAccurately()
    {
        // Arrange
        var service = CreateService();
        var cat1 = new ExpenseCategory { Id = "cat_med", CharityId = "c1", Name = "إعانات علاجية", Code = "EXP-001" };
        var cat2 = new ExpenseCategory { Id = "cat_edu", CharityId = "c1", Name = "مصاريف تعليمية", Code = "EXP-002" };
        await _categoryRepo.AddAsync(cat1);
        await _categoryRepo.AddAsync(cat2);

        // Add 2 distributions for cat1 and none for cat2
        await _distributionRepo.AddAsync(new Distribution { Id = "d1", BeneficiaryId = "b1", ExpenseCategoryId = "cat_med", Amount = 1200, CharityId = "c1" });
        await _distributionRepo.AddAsync(new Distribution { Id = "d2", BeneficiaryId = "b2", ExpenseCategoryId = "cat_med", Amount = 800, CharityId = "c1" });

        // Act
        var list = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, list.Count);
        
        var medResult = list.First(c => c.Id == "cat_med");
        Assert.Equal(2000, medResult.TotalDisbursedAmount);
        Assert.Equal(2, medResult.DisbursementsCount);
        Assert.Equal(2, medResult.BeneficiariesCount);
        Assert.False(medResult.CanDelete); // Has operations, delete blocked

        var eduResult = list.First(c => c.Id == "cat_edu");
        Assert.Equal(0, eduResult.TotalDisbursedAmount);
        Assert.Equal(0, eduResult.DisbursementsCount);
        Assert.Equal(0, eduResult.BeneficiariesCount);
        Assert.True(eduResult.CanDelete); // No operations, can delete
    }

    [Fact]
    public async Task GetDisbursementsByCategoryAsync_ReturnsDetailedBeneficiariesAndAmounts()
    {
        // Arrange
        var service = CreateService();
        var cat = new ExpenseCategory { Id = "cat_debt", Name = "سداد ديون وغارمين", Code = "EXP-005", CharityId = "c1" };
        await _categoryRepo.AddAsync(cat);

        var ben = new Beneficiary
        {
            Id = "ben_1",
            Name = "فاطمة إبراهيم",
            FamilyCode = "FAM-00042",
            FamilyName = "عائلة إبراهيم",
            NationalId = "29505051234567",
            Phone = "01012345678",
            GovernorateName = "كفر الشيخ",
            DistrictName = "مطوبس"
        };
        await _beneficiaryRepo.AddAsync(ben);

        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "dist_1",
            BeneficiaryId = "ben_1",
            ExpenseCategoryId = "cat_debt",
            Amount = 5000,
            Date = "2026-10-15",
            CharityId = "c1",
            ReceivedByName = "فاطمة إبراهيم",
            ReceivedByRelationship = "صاحب الملف (رب الأسرة)",
            ReceivedByNationalId = "29505051234567",
            CharityName = "جمعية الأمل الخيرية",
            Notes = "سداد إيصال أمانة لفك كرب"
        });

        // Act
        var details = await service.GetDisbursementsByCategoryAsync("cat_debt");

        // Assert
        Assert.Single(details);
        var item = details[0];
        Assert.Equal("ben_1", item.BeneficiaryId);
        Assert.Equal("فاطمة إبراهيم", item.BeneficiaryName);
        Assert.Equal("FAM-00042", item.FamilyCode);
        Assert.Equal(5000, item.Amount);
        Assert.Equal("2026-10-15", item.Date);
        Assert.Equal("سداد إيصال أمانة لفك كرب", item.Notes);
        Assert.Equal("كفر الشيخ", item.GovernorateName);
        Assert.Equal("مطوبس", item.DistrictName);
    }

    [Fact]
    public async Task DeleteAsync_UnusedCategory_DeletesSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var cat = new ExpenseCategory { Id = "cat_unused", Name = "بند غير مستخدم", Code = "EXP-099", CharityId = "c1" };
        await _categoryRepo.AddAsync(cat);

        // Act
        await service.DeleteAsync("cat_unused");

        // Assert
        Assert.Equal(1, _categoryRepo.DeleteCalls);
        Assert.Equal(1, _uow.SaveCalls);
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithDistributions_ThrowsValidationDomainExceptionAndBlocksDeletion()
    {
        // Arrange
        var service = CreateService();
        var cat = new ExpenseCategory { Id = "cat_active", Name = "إعانات طارئة", Code = "EXP-006", CharityId = "c1" };
        await _categoryRepo.AddAsync(cat);

        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "d_past",
            ExpenseCategoryId = "cat_active",
            BeneficiaryId = "b1",
            Amount = 2500,
            CharityId = "c1"
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationDomainException>(() => service.DeleteAsync("cat_active"));
        Assert.Contains("محظور الحذف", ex.Message);
        Assert.Contains("إعانات طارئة", ex.Message);
        Assert.Contains("حركة صرف سابقة", ex.Message);
        Assert.Equal(0, _categoryRepo.DeleteCalls); // Did NOT delete!
    }

    [Fact]
    public async Task GetAllAsync_WithMonthFilter_FiltersAccurately()
    {
        // Arrange
        var service = CreateService();
        var cat = new ExpenseCategory { Id = "cat_mon", Name = "إعانة دورية", Code = "EXP-010", CharityId = "c1" };
        await _categoryRepo.AddAsync(cat);

        // Disbursed in August 2026
        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "d_aug",
            ExpenseCategoryId = "cat_mon",
            BeneficiaryId = "b1",
            Amount = 1500,
            Date = "2026-08-15",
            CharityId = "c1",
            Kind = "إعانة نقدية"
        });

        // Disbursed in September 2026
        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "d_sep",
            ExpenseCategoryId = "cat_mon",
            BeneficiaryId = "b2",
            Amount = 2500,
            Date = "2026-09-10",
            CharityId = "c1",
            Kind = "إعانة نقدية"
        });

        // Act - query for August
        var listAug = await service.GetAllAsync("2026-08");
        var augItem = listAug.First(c => c.Id == "cat_mon");

        // Assert - August
        Assert.Equal(1500, augItem.TotalDisbursedAmount);
        Assert.Equal(1, augItem.DisbursementsCount);

        // Act - query for September
        var listSep = await service.GetAllAsync("2026-09");
        var sepItem = listSep.First(c => c.Id == "cat_mon");

        // Assert - September
        Assert.Equal(2500, sepItem.TotalDisbursedAmount);
        Assert.Equal(1, sepItem.DisbursementsCount);

        // Act - query all
        var listAll = await service.GetAllAsync();
        var allItem = listAll.First(c => c.Id == "cat_mon");
        Assert.Equal(4000, allItem.TotalDisbursedAmount);
        Assert.Equal(2, allItem.DisbursementsCount);
    }

    [Fact]
    public async Task GetMonthlyMovementsAsync_GroupsByMonthDescending()
    {
        // Arrange
        var service = CreateService();
        var cat = new ExpenseCategory { Id = "cat_cash", Name = "إعانة نقدية شهرية", Code = "EXP-011", CharityId = "c1" };
        await _categoryRepo.AddAsync(cat);

        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "d_jul",
            ExpenseCategoryId = "cat_cash",
            ExpenseCategoryName = "إعانة نقدية شهرية",
            BeneficiaryId = "b1",
            Amount = 3000,
            Date = "2026-07-20",
            CharityId = "c1",
            Kind = "إعانة نقدية"
        });

        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "d_aug_1",
            ExpenseCategoryId = "cat_cash",
            ExpenseCategoryName = "إعانة نقدية شهرية",
            BeneficiaryId = "b1",
            Amount = 2000,
            Date = "2026-08-05",
            CharityId = "c1",
            Kind = "إعانة نقدية"
        });

        await _distributionRepo.AddAsync(new Distribution
        {
            Id = "d_aug_2",
            ExpenseCategoryId = "cat_cash",
            ExpenseCategoryName = "إعانة نقدية شهرية",
            BeneficiaryId = "b2",
            Amount = 1500,
            Date = "2026-08-25",
            CharityId = "c1",
            Kind = "إعانة نقدية"
        });

        // Act
        var movements = await service.GetMonthlyMovementsAsync();

        // Assert
        Assert.True(movements.Count >= 2);
        var aug = movements.First(m => m.MonthYear == "2026-08");
        Assert.Equal("شهر 8/2026", aug.MonthLabel);
        Assert.Equal(3500, aug.TotalAmount);
        Assert.Equal(2, aug.DisbursementsCount);
        Assert.Equal(2, aug.BeneficiariesCount);

        var jul = movements.First(m => m.MonthYear == "2026-07");
        Assert.Equal("شهر 7/2026", jul.MonthLabel);
        Assert.Equal(3000, jul.TotalAmount);
        Assert.Equal(1, jul.DisbursementsCount);
    }

    [Fact]
    public async Task HistoricalCashKindsAppearInMonthlyReportAndDateRangeMatchesDetails()
    {
        var service = CreateService();
        await _categoryRepo.AddAsync(new ExpenseCategory { Id = "medical", Name = "علاج", Code = "EXP-001", CharityId = "c1" });
        await _distributionRepo.AddAsync(new Distribution { Id = "old", BeneficiaryId = "family-1", ExpenseCategoryId = "medical", ExpenseCategoryName = "علاج", Kind = "نقدية", Amount = 1200, Date = "2026-10-20", CharityId = "c1" });
        await _distributionRepo.AddAsync(new Distribution { Id = "new", BeneficiaryId = "family-2", ExpenseCategoryId = "medical", ExpenseCategoryName = "علاج", Kind = "إعانة نقدية", Amount = 5000, Date = "2026-10-21", CharityId = "c1" });

        var months = await service.GetMonthlyMovementsAsync(2026);
        Assert.Equal(6200, Assert.Single(months).TotalAmount);
        Assert.Equal(2, Assert.Single(months).BeneficiariesCount);

        var range = await service.GetAllAsync(from: "2026-10-20", to: "2026-10-20");
        var category = Assert.Single(range);
        Assert.Equal(1200, category.TotalDisbursedAmount);
        Assert.Single(await service.GetDisbursementsByCategoryAsync("medical", from: "2026-10-20", to: "2026-10-20"));
        Assert.False(category.CanDelete);
        Assert.Empty(await service.GetMonthlyMovementsAsync(from: "2026-10-22", to: "2026-10-23"));
    }

    [Theory]
    [InlineData("2026-10-22", "2026-10-20")]
    [InlineData("2026-02-30", "2026-03-01")]
    public async Task InvalidReportDateRangeIsRejected(string from, string to)
    {
        await Assert.ThrowsAsync<ValidationDomainException>(() => CreateService().GetAllAsync(from: from, to: to));
    }
}
