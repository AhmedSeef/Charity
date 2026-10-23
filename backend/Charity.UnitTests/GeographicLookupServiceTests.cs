using Charity.Application.DTOs.Lookups;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Xunit;

namespace Charity.UnitTests;

public class GeographicLookupServiceTests
{
    [Theory]
    [InlineData("كفر الشيخ", "كفر الشيخ")]
    [InlineData("كَفْرُ الشَّيْخِ", "كفر الشيخ")] // تشكيل
    [InlineData("كـفــر الـشـيـخ", "كفر الشيخ")] // تطويل
    [InlineData("الإسكندرية", "الاسكندريه")] // همزة وتاء مربوطة
    [InlineData("أبيانة", "ابيانه")] // همزة وتاء مربوطة
    [InlineData("منية المرشد", "منيه المرشد")]
    [InlineData("برنبال (برمبال)", "برنبال برمبال")] // أقواس
    [InlineData("CAIRO - Egypt", "cairo egypt")] // إنجليزي
    public void Normalize_HandlesArabicAndEnglishCorrectly(string input, string expected)
    {
        var result = AddressNormalizationService.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateMatchScore_ExactMatch_ReturnsMaxScore()
    {
        var candidate = "قرية الجزيرة الخضراء";
        var candidateNorm = AddressNormalizationService.Normalize(candidate);
        var queryNorm = AddressNormalizationService.Normalize("الجزيرة الخضراء");

        var score = AddressNormalizationService.CalculateMatchScore(candidate, candidateNorm, queryNorm, usageCount: 5);
        Assert.True(score >= 70);
    }

    [Fact]
    public void CalculateMatchScore_FuzzyCloseMatch_ReturnsValidScore()
    {
        // "جزيره خضرا" بدون تعريف وبتاء هاء مقابل "قرية الجزيرة الخضراء"
        var candidate = "قرية الجزيرة الخضراء";
        var candidateNorm = AddressNormalizationService.Normalize(candidate);
        var queryNorm = AddressNormalizationService.Normalize("جزيره خضرا");

        var score = AddressNormalizationService.CalculateMatchScore(candidate, candidateNorm, queryNorm, usageCount: 2);
        Assert.True(score >= 40, $"Expected score >= 40 but got {score}");
    }

    [Fact]
    public async Task AutocompleteAddresses_FiltersAndRanksByScore()
    {
        // Arrange
        var govRepo = new FakeRepository<Governorate>(
            new Governorate { Id = "gov_kfs", Name = "كفر الشيخ" }
        );
        var distRepo = new FakeRepository<District>(
            new District { Id = "dist_motobas", GovernorateId = "gov_kfs", Name = "مركز مطوبس" }
        );
        var addrRepo = new FakeRepository<AddressRecord>(
            new AddressRecord
            {
                Id = "a1",
                CharityId = "c1",
                GovernorateId = "gov_kfs",
                DistrictId = "dist_motobas",
                AddressText = "قرية الجزيرة الخضراء",
                NormalizedText = AddressNormalizationService.Normalize("قرية الجزيرة الخضراء"),
                UsageCount = 10
            },
            new AddressRecord
            {
                Id = "a2",
                CharityId = "c1",
                GovernorateId = "gov_kfs",
                DistrictId = "dist_motobas",
                AddressText = "قرية برج مغيزل",
                NormalizedText = AddressNormalizationService.Normalize("قرية برج مغيزل"),
                UsageCount = 5
            },
            new AddressRecord
            {
                Id = "a3",
                CharityId = "c1",
                GovernorateId = "gov_kfs",
                DistrictId = "dist_motobas",
                AddressText = "قرية معدية مهدي",
                NormalizedText = AddressNormalizationService.Normalize("قرية معدية مهدي"),
                UsageCount = 2
            }
        );

        var service = new GeographicLookupService(
            govRepo,
            distRepo,
            new FakeRepository<Village>(),
            new FakeRepository<VillageArea>(),
            new FakeRepository<Beneficiary>(),
            addrRepo,
            new FakeCurrentUser(),
            new FakeUnitOfWork()
        );

        // Act - search with typo / partial
        var results = await service.AutocompleteAddressesAsync("الجزيره", "gov_kfs", "dist_motobas");

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal("قرية الجزيرة الخضراء", results[0].AddressText);
    }

    [Fact]
    public async Task RecordAddressUsage_IncrementsUsageWhenExists()
    {
        var addrRepo = new FakeRepository<AddressRecord>(
            new AddressRecord
            {
                Id = "a1",
                CharityId = "c1",
                GovernorateId = "gov_kfs",
                DistrictId = "dist_motobas",
                AddressText = "قرية برنبال",
                NormalizedText = AddressNormalizationService.Normalize("قرية برنبال"),
                UsageCount = 1
            }
        );

        var service = new GeographicLookupService(
            new FakeRepository<Governorate>(),
            new FakeRepository<District>(),
            new FakeRepository<Village>(),
            new FakeRepository<VillageArea>(),
            new FakeRepository<Beneficiary>(),
            addrRepo,
            new FakeCurrentUser(),
            new FakeUnitOfWork()
        );

        var dto = new CreateAddressRecordDto
        {
            GovernorateId = "gov_kfs",
            DistrictId = "dist_motobas",
            AddressText = "قرية برنبال"
        };

        var recorded = await service.RecordAddressUsageAsync(dto);

        Assert.Equal(2, recorded.UsageCount);
    }

    [Fact]
    public async Task VillagesAndAreas_CRUD_WorksAsExpected()
    {
        var distRepo = new FakeRepository<District>(
            new District { Id = "dist_motobas", GovernorateId = "gov_kfs", Name = "مركز مطوبس" }
        );
        var villageRepo = new FakeRepository<Village>();
        var areaRepo = new FakeRepository<VillageArea>();

        var service = new GeographicLookupService(
            new FakeRepository<Governorate>(),
            distRepo,
            villageRepo,
            areaRepo,
            new FakeRepository<Beneficiary>(),
            new FakeRepository<AddressRecord>(),
            new FakeCurrentUser(),
            new FakeUnitOfWork()
        );

        // 1. Create Village
        var villageDto = await service.CreateVillageAsync(new CreateVillageDto
        {
            DistrictId = "dist_motobas",
            Name = "منية المرشد",
            NameEn = "Monyat El-Morshid"
        });

        Assert.NotNull(villageDto.Id);
        Assert.Equal("منية المرشد", villageDto.Name);

        // 2. Create Area under Village
        var areaDto = await service.CreateVillageAreaAsync(new CreateVillageAreaDto
        {
            VillageId = villageDto.Id,
            Name = "المنشر",
            NameEn = "Al-Manshar",
            Notes = "منطقة المنشر"
        });

        Assert.NotNull(areaDto.Id);
        Assert.Equal("المنشر", areaDto.Name);

        // 3. Get Areas for Village
        var areas = await service.GetVillageAreasAsync(villageDto.Id);
        Assert.Single(areas);
        Assert.Equal("المنشر", areas[0].Name);

        // 4. Update Area
        var updatedArea = await service.UpdateVillageAreaAsync(areaDto.Id, new UpdateVillageAreaDto
        {
            Name = "المنشر الجديد",
            IsActive = true
        });
        Assert.Equal("المنشر الجديد", updatedArea.Name);

        // 5. Delete Area
        await service.DeleteVillageAreaAsync(areaDto.Id);
        var areasAfterDelete = await service.GetVillageAreasAsync(villageDto.Id);
        Assert.Empty(areasAfterDelete);
    }
}
