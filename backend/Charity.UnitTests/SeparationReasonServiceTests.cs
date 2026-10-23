using Charity.Application.DTOs.SeparationReasons;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Xunit;

namespace Charity.UnitTests;

public class SeparationReasonServiceTests
{
    private readonly FakeRepository<SeparationReason> _sepRepo = new();
    private readonly FakeRepository<BeneficiaryFamilyMember> _memberRepo = new();
    private readonly FakeRepository<Beneficiary> _benRepo = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly FakeUnitOfWork _uow = new();

    private SeparationReasonService CreateService()
    {
        return new SeparationReasonService(_sepRepo, _memberRepo, _benRepo, _currentUser, _uow);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_AddsSeparationReason()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateSeparationReasonDto
        {
            Title = "زواج واستقلال بمسكن جديد",
            Description = "تأسيس أسرة جديدة مستقلة"
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("زواج واستقلال بمسكن جديد", result.Title);
        Assert.True(result.IsActive);
        Assert.Equal("c1", result.CharityId);
    }

    [Fact]
    public async Task DeleteAsync_UnusedReason_DeletesSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var reason = new SeparationReason
        {
            Id = "sep_unused",
            CharityId = "c1",
            Title = "سبب تجريبي لم يُستخدم",
            Description = "وصف"
        };
        await _sepRepo.AddAsync(reason);

        // Act
        await service.DeleteAsync("sep_unused");

        // Assert
        Assert.Equal(1, _sepRepo.DeleteCalls);
    }

    [Fact]
    public async Task DeleteAsync_ReasonUsedInOperation_ThrowsValidationDomainException()
    {
        // Arrange
        var service = CreateService();
        var reason = new SeparationReason
        {
            Id = "sep_used",
            CharityId = "c1",
            Title = "استقلال بعد الزواج",
            Description = "تم تنفيذ عملية فصل بناء عليه"
        };
        await _sepRepo.AddAsync(reason);

        // Member separated using this reason
        var member = new BeneficiaryFamilyMember
        {
            Id = "m1",
            Name = "أحمد محمد",
            NationalId = "29001011234567",
            BeneficiaryId = "b1",
            IsSeparated = true,
            SeparationReasonId = "sep_used",
            SeparationReason = "استقلال بعد الزواج"
        };
        await _memberRepo.AddAsync(member);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationDomainException>(() => service.DeleteAsync("sep_used"));
        Assert.Contains("محظور الحذف", ex.Message);
        Assert.Contains("عملية انفصال سابقة", ex.Message);
    }
}
