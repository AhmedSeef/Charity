using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Xunit;

namespace Charity.UnitTests;

public class BeneficiaryTransferHeadshipTests
{
    private readonly FakeRepository<Beneficiary> _beneficiaryRepo = new();
    private readonly FakeRepository<BeneficiaryFamilyMember> _familyMemberRepo = new();
    private readonly FakeRepository<BeneficiaryHeadHistory> _headHistoryRepo = new();
    private readonly FakeRepository<Distribution> _distributionRepo = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private BeneficiaryService CreateService()
    {
        return new BeneficiaryService(
            _beneficiaryRepo,
            _familyMemberRepo,
            _headHistoryRepo,
            _distributionRepo,
            _currentUser,
            _unitOfWork);
    }

    [Fact]
    public async Task TransferHeadship_KeepAsFamilyMember_PreservesFormerHeadAsLivingActiveMember()
    {
        // Arrange
        var service = CreateService();
        var ben = new Beneficiary
        {
            Id = "ben1",
            Name = "محمد محمود حسن",
            NationalId = "28501011234567",
            FamilyName = "عائلة محمد محمود حسن",
            FamilyCode = "FAM-26-0001",
            FamilyCount = 2,
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الاختبار"
        };
        await _beneficiaryRepo.AddAsync(ben);

        var wife = new BeneficiaryFamilyMember
        {
            Id = "fm_wife",
            BeneficiaryId = "ben1",
            Name = "مريم السيد إبراهيم",
            NationalId = "28804041234568",
            Relationship = "زوجة",
            MemberType = "معال",
            SupportType = "ربة منزل"
        };
        await _familyMemberRepo.AddAsync(wife);

        var dto = new TransferHeadshipDto
        {
            NewHeadFamilyMemberId = "fm_wife",
            FormerHeadAction = "KeepAsFamilyMember",
            Reason = "عجز كلي أو مرض مزمن مقعد ♿ (العائل حي ومقيم كمعال)",
            FormerHeadRelationship = "زوج (عائل سابق / مريض معال)",
            FormerHeadSupportType = "مريض / عاجز عن العمل"
        };

        // Act
        var result = await service.TransferHeadshipAsync("ben1", dto);

        // Assert
        // 1. New master beneficiary is the wife
        Assert.Equal("مريم السيد إبراهيم", result.Name);
        Assert.Equal("28804041234568", result.NationalId);
        Assert.False(result.FamilyName.Contains("المرحوم"), "Living former head must not add 'المرحوم' to family name");

        // 2. Former head is in family members as an active living dependent
        var formerHeadMember = _familyMemberRepo.Items.FirstOrDefault(m => m.NationalId == "28501011234567");
        Assert.NotNull(formerHeadMember);
        Assert.False(formerHeadMember.IsDeleted);
        Assert.True(formerHeadMember.IsFormerHead);
        Assert.Equal("زوج (عائل سابق / مريض معال)", formerHeadMember.Relationship);
        Assert.Equal("مريض / عاجز عن العمل", formerHeadMember.SupportType);

        // 3. Wife is removed from family members table since she's now the master
        var wifeInMembers = _familyMemberRepo.Items.FirstOrDefault(m => m.Id == "fm_wife");
        Assert.Null(wifeInMembers);

        // 4. Family count includes both (wife master + living former head member = 2)
        Assert.Equal(2, result.FamilyCount);

        // 5. History recorded
        var history = _headHistoryRepo.Items.FirstOrDefault(h => h.BeneficiaryId == "ben1");
        Assert.NotNull(history);
        Assert.Equal("محمد محمود حسن", history.PreviousHeadName);
        Assert.Equal("مريم السيد إبراهيم", history.NewHeadName);
        Assert.Contains("نقل ولاية مع بقاء العائل السابق", history.Notes);
    }

    [Fact]
    public async Task TransferHeadship_RemoveFromFamily_RemovesFormerHeadAndMarksDeceasedIfDeath()
    {
        // Arrange
        var service = CreateService();
        var ben = new Beneficiary
        {
            Id = "ben2",
            Name = "أحمد مصطفى كامل",
            NationalId = "27001011234560",
            FamilyName = "عائلة أحمد مصطفى كامل",
            FamilyCode = "FAM-26-0002",
            FamilyCount = 2,
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الاختبار"
        };
        await _beneficiaryRepo.AddAsync(ben);

        var son = new BeneficiaryFamilyMember
        {
            Id = "fm_son",
            BeneficiaryId = "ben2",
            Name = "طارق أحمد مصطفى",
            NationalId = "30005051234561",
            Relationship = "ابن",
            MemberType = "معال",
            SupportType = "طالب"
        };
        await _familyMemberRepo.AddAsync(son);

        var dto = new TransferHeadshipDto
        {
            NewHeadFamilyMemberId = "fm_son",
            FormerHeadAction = "RemoveFromFamily",
            Reason = "وفاة رب الأسرة السابق 🕊️ (رحمه الله)"
        };

        // Act
        var result = await service.TransferHeadshipAsync("ben2", dto);

        // Assert
        // 1. New master beneficiary is the son
        Assert.Equal("طارق أحمد مصطفى", result.Name);
        Assert.Equal("30005051234561", result.NationalId);
        Assert.Contains("المرحوم", result.FamilyName);

        // 2. Active family members count is 1 (only the son now as master)
        Assert.Equal(1, result.FamilyCount);

        // 3. History recorded
        var history = _headHistoryRepo.Items.FirstOrDefault(h => h.BeneficiaryId == "ben2");
        Assert.NotNull(history);
        Assert.Equal("أحمد مصطفى كامل", history.PreviousHeadName);
        Assert.Equal("طارق أحمد مصطفى", history.NewHeadName);
        Assert.Contains("استبعاد", history.Notes);
    }

    [Fact]
    public async Task TransferHeadship_UnauthorizedCharity_ThrowsUnauthorizedDomainException()
    {
        // Arrange
        var service = CreateService();
        var ben = new Beneficiary
        {
            Id = "ben3",
            Name = "شخص في جمعية أخرى",
            NationalId = "28001011234567",
            RegisteredByCharityId = "c2_other",
            RegisteredByCharityName = "جمعية أخرى"
        };
        await _beneficiaryRepo.AddAsync(ben);

        var dto = new TransferHeadshipDto
        {
            NewHeadFamilyMemberId = "fm_any",
            FormerHeadAction = "KeepAsFamilyMember"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.TransferHeadshipAsync("ben3", dto));
    }

    [Fact]
    public async Task GetNextFamilyCodeAsync_WhenEmpty_ReturnsFirstSequentialCode()
    {
        // Arrange
        var service = CreateService();
        var year = DateTime.UtcNow.ToString("yy");

        // Act
        var code = await service.GetNextFamilyCodeAsync();

        // Assert
        Assert.Equal($"FAM-{year}-0001", code);
    }

    [Fact]
    public async Task GetNextFamilyCodeAsync_WhenExistingCodesPresent_IncrementsSequentially()
    {
        // Arrange
        var service = CreateService();
        var year = DateTime.UtcNow.ToString("yy");

        await _beneficiaryRepo.AddAsync(new Beneficiary
        {
            Id = "b1",
            Name = "أسرة 1",
            NationalId = "28001011234561",
            FamilyCode = $"FAM-{year}-0001"
        });
        await _beneficiaryRepo.AddAsync(new Beneficiary
        {
            Id = "b2",
            Name = "أسرة 2",
            NationalId = "28001011234562",
            FamilyCode = $"FAM-{year}-0002"
        });

        // Act
        var nextCode = await service.GetNextFamilyCodeAsync();

        // Assert
        Assert.Equal($"FAM-{year}-0003", nextCode);
    }

    [Fact]
    public async Task CreateAsync_AutomaticallyAssignsUnifiedFamilyCode()
    {
        // Arrange
        var service = CreateService();
        var year = DateTime.UtcNow.ToString("yy");

        var createDto = new CreateBeneficiaryDto
        {
            Name = "أسرة تجريبية جديدة",
            NationalId = "29001011234563",
            Category = "أسر أولى بالرعاية",
            Phone = "01012345678",
            FamilyCode = "" // ترك الكود فارغاً للاعتماد على التوليد التلقائي
        };

        // Act
        var created = await service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(created.FamilyCode);
        Assert.StartsWith($"FAM-{year}-", created.FamilyCode);
        Assert.Equal($"FAM-{year}-0001", created.FamilyCode);
    }
}

