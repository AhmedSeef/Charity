using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Donations;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Lookups;
using Charity.Application.DTOs.Staff;
using Charity.Application.Services.Implementations;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Security;
using Xunit;

namespace Charity.UnitTests;

public class RoleAndPermissionSecurityTests
{
    [Fact]
    public async Task StaffWithoutViewCannotReadBeneficiaryFiles()
    {
        var (service, _, _) = CreateBeneficiaryService("Staff");
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.GetAllAsync());
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.GetByIdAsync("any"));
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.GetAidSummaryAsync("any"));
    }

    [Fact]
    public async Task ViewOnlyStaffCannotChangeBeneficiaryStatusOrFamily()
    {
        var (service, _, _) = CreateBeneficiaryService("Staff", AppPermissions.BeneficiariesView);
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.SuspendAsync("any", new SuspendBeneficiaryDto()));
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.ReactivateAsync("any"));
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.AddFamilyMemberAsync("any", new CreateFamilyMemberDto()));
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.SeparateFamilyMemberAsync("any", new SeparateFamilyMemberDto()));
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.TransferHeadshipAsync("any", new TransferHeadshipDto()));
    }

    [Fact]
    public async Task StaffWithoutExpensesManageCannotReadOrChangeExpenseCategories()
    {
        var user = new FakeCurrentUser { Role = "Staff" };
        var service = new ExpenseCategoryService(new FakeRepository<ExpenseCategory>(),
            new FakeRepository<Distribution>(), new FakeRepository<Beneficiary>(), user, new FakeUnitOfWork());
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.GetAllAsync());
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.GetMonthlyMovementsAsync());
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.CreateAsync(new CreateExpenseCategoryDto { Name = "اختبار" }));
        await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.DeleteAsync("any"));
    }

    private static (BeneficiaryService service, FakeRepository<Beneficiary> repo, FakeCurrentUser user) CreateBeneficiaryService(string role, params string[] perms)
    {
        var benRepo = new FakeRepository<Beneficiary>();
        var fmRepo = new FakeRepository<BeneficiaryFamilyMember>();
        var headRepo = new FakeRepository<BeneficiaryHeadHistory>();
        var distRepo = new FakeRepository<Distribution>();
        var user = new FakeCurrentUser { Role = role };
        user.GrantedPermissions.AddRange(perms);
        var uow = new FakeUnitOfWork();
        var service = new BeneficiaryService(benRepo, fmRepo, headRepo, distRepo, user, uow);
        return (service, benRepo, user);
    }

    [Fact]
    public async Task BeneficiaryViewFull_WhenRestricted_MasksSensitiveDebtAndHealthData()
    {
        // DataEntry role: has view & create, but NO view_full
        var (service, repo, _) = CreateBeneficiaryService("Staff", AppPermissions.BeneficiariesView, AppPermissions.BeneficiariesCreate);
        var ben = new Beneficiary
        {
            Id = "b1",
            Name = "محمد عبد الفتاح",
            NationalId = "29001011234567",
            DebtNotes = "مدين بمبلغ 25000 لصالح تاجر حبوب بموجب إيصالات أمانة",
            TotalDebtAmount = 25000,
            DebtCreditor = "الحاج إبراهيم التاجر",
            HealthIssues = "ورم كبدي ومحتاج جلسات علاج كيماوي شهرياً",
            ChronicDiseaseDetails = "فيروس سي مزمن",
            ResearchNotes = "الأسرة تعيش في فقر مدقع ولا يوجد معيل",
            MonthlyIncome = 1200,
            IncomeSource = "يومية عمالة غير منتظمة",
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الأمل"
        };
        await repo.AddAsync(ben);

        var result = await service.GetByIdAsync("b1");

        Assert.NotNull(result);
        Assert.Equal("محمد عبد الفتاح", result.Name);
        Assert.Equal("29001011234567", result.NationalId);
        // Sensitive data MUST be masked
        Assert.Equal("🔒 بيانات محجوبة (صلاحية خاصة)", result.DebtNotes);
        Assert.Null(result.TotalDebtAmount);
        Assert.Equal("🔒 محجوب", result.DebtCreditor);
        Assert.Equal("🔒 بيانات طبية خاصة محجوبة", result.HealthIssues);
        Assert.Equal("🔒 محجوب", result.ChronicDiseaseDetails);
        Assert.Equal("🔒 تقرير البحث الاجتماعي محجوب لدواعي الخصوصية", result.ResearchNotes);
        Assert.Null(result.MonthlyIncome);
        Assert.Equal("🔒 محجوب", result.IncomeSource);
    }

    [Fact]
    public async Task BeneficiaryViewFull_WhenGranted_ShowsFullSensitiveData()
    {
        // SocialWorker: has view_full
        var (service, repo, _) = CreateBeneficiaryService("Staff",
            AppPermissions.BeneficiariesView,
            AppPermissions.BeneficiariesViewFull);

        var ben = new Beneficiary
        {
            Id = "b2",
            Name = "سيدة حسن",
            NationalId = "28502021234568",
            DebtNotes = "ديون علاج مستشفيات",
            TotalDebtAmount = 15000,
            DebtCreditor = "صيدلية الإيمان",
            HealthIssues = "فشل كلوي حاد",
            ResearchNotes = "حالة عاجلة جداً للدعم المالي",
            MonthlyIncome = 800,
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الأمل"
        };
        await repo.AddAsync(ben);

        var result = await service.GetByIdAsync("b2");

        Assert.NotNull(result);
        Assert.Equal("ديون علاج مستشفيات", result.DebtNotes);
        Assert.Equal(15000, result.TotalDebtAmount);
        Assert.Equal("صيدلية الإيمان", result.DebtCreditor);
        Assert.Equal("فشل كلوي حاد", result.HealthIssues);
        Assert.Equal("حالة عاجلة جداً للدعم المالي", result.ResearchNotes);
        Assert.Equal(800, result.MonthlyIncome);
    }

    [Fact]
    public async Task BeneficiaryDelete_WhenUserLacksPermission_ThrowsUnauthorized()
    {
        // User has only view and edit, NO delete
        var (service, repo, _) = CreateBeneficiaryService("Staff", AppPermissions.BeneficiariesView, AppPermissions.BeneficiariesEdit);
        var ben = new Beneficiary
        {
            Id = "b3",
            Name = "علي محمود",
            NationalId = "29303031234569",
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الأمل"
        };
        await repo.AddAsync(ben);

        var ex = await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.DeleteAsync("b3"));
        Assert.Contains("ليس لديك صلاحية حذف", ex.Message);
    }

    [Fact]
    public async Task DisbursementExecute_WhenUserLacksPermission_ThrowsUnauthorized()
    {
        var user = new FakeCurrentUser { Role = "Staff" }; // No disbursement.execute
        var distRepo = new FakeRepository<Distribution>();
        var invRepo = new FakeRepository<InventoryItem>();
        var benRepo = new FakeRepository<Beneficiary>();
        var fmRepo = new FakeRepository<BeneficiaryFamilyMember>();
        var expRepo = new FakeRepository<ExpenseCategory>();
        var donRepo = new FakeRepository<Donation>();
        var uow = new FakeUnitOfWork();

        var service = new DistributionService(distRepo, invRepo, benRepo, fmRepo, expRepo, user, uow, donRepo);

        var dto = new CreateDistributionDto
        {
            BeneficiaryId = "b1",
            Kind = "إعانة نقدية",
            Amount = 1000,
            Quantity = 1,
            Date = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };

        var ex = await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.CreateDistributionAsync(dto));
        Assert.Contains("ليس لديك صلاحية تنفيذ صرف المساعدات", ex.Message);
    }

    [Fact]
    public async Task InventoryManage_WhenUserLacksPermission_ThrowsUnauthorized()
    {
        var user = new FakeCurrentUser { Role = "Staff" };
        user.GrantedPermissions.Add(AppPermissions.InventoryView); // View only, no manage
        var invRepo = new FakeRepository<InventoryItem>();
        var uow = new FakeUnitOfWork();

        var service = new InventoryService(invRepo, user, uow);

        var createDto = new CreateInventoryItemDto
        {
            ItemName = "كرتونة مواد غذائية",
            Kind = "صنف ملموس",
            Quantity = 100,
            Unit = "كرتونة"
        };

        var ex = await Assert.ThrowsAsync<UnauthorizedDomainException>(() => service.CreateAsync(createDto));
        Assert.Contains("ليس لديك صلاحية إضافة وتعديل الأصناف", ex.Message);
    }

    [Fact]
    public async Task CharityManager_HasAllPermissionsImplicitly()
    {
        // CharityManager does not need explicit permission strings; HasPermission always returns true
        var (service, repo, _) = CreateBeneficiaryService("CharityManager");
        var ben = new Beneficiary
        {
            Id = "bm1",
            Name = "أحمد رضوان",
            NationalId = "29404041234560",
            DebtNotes = "سرية",
            TotalDebtAmount = 5000,
            HealthIssues = "مرض مزمن",
            ResearchNotes = "ملاحظات تفصيلية",
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الأمل"
        };
        await repo.AddAsync(ben);

        var result = await service.GetByIdAsync("bm1");
        Assert.NotNull(result);
        Assert.Equal("سرية", result.DebtNotes);
        Assert.Equal(5000, result.TotalDebtAmount);
        Assert.Equal("مرض مزمن", result.HealthIssues);

        // Can also delete
        await service.DeleteAsync("bm1");
        var deleted = await repo.GetByIdAsync("bm1");
        Assert.Null(deleted); // Soft deleted
    }
}
