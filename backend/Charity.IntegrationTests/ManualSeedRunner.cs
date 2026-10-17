using Charity.Domain.Entities;
using Charity.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Charity.IntegrationTests;

public class ManualSeedRunner
{
    private const string ConnectionString = "Server=.;Database=CharityDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

    [Fact(Skip = "Used to re-seed demo data")]
    public async Task SeedTwoCharitiesAndDataIntoProductionDatabase()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());
        services.AddDbContext<CharityDbContext>(options =>
            options.UseSqlServer(ConnectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 3;
        })
        .AddEntityFrameworkStores<CharityDbContext>()
        .AddDefaultTokenProviders();

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CharityDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Roles
        string[] roles = ["Admin", "CharityManager", "Staff"];
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }

        // 2. Ensure Super Admin exists
        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@charity.org",
                FullName = "مدير شبكة الجمعيات",
                CharityId = null,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "admin");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            admin.FullName = "مدير شبكة الجمعيات";
            admin.CharityId = null;
            admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, "admin");
            await userManager.UpdateAsync(admin);
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // 3. Clear existing operational test data so we have a completely clean, predictable state
        db.Distributions.RemoveRange(db.Distributions);
        db.BeneficiaryHeadHistories.RemoveRange(db.BeneficiaryHeadHistories);
        db.BeneficiaryFamilyMembers.RemoveRange(db.BeneficiaryFamilyMembers);
        db.Beneficiaries.RemoveRange(db.Beneficiaries);
        db.InventoryItems.RemoveRange(db.InventoryItems);
        db.Donations.RemoveRange(db.Donations);
        db.StaffMembers.RemoveRange(db.StaffMembers);
        await db.SaveChangesAsync();

        // 4. Seed 2 Charities
        var c1 = await db.Charities.FirstOrDefaultAsync(c => c.Id == "c1");
        if (c1 == null)
        {
            c1 = new Charity.Domain.Entities.Charity
            {
                Id = "c1",
                Name = "جمعية الأمل لرعاية الأيتام",
                RoleDesc = "كفالة الأيتام والإعانات النقدية والغذائية وتجهيز العرائس",
                Color = "#1565c0",
                Icon = "🤝",
                Phone = "01012345678",
                Address = "القاهرة، المعادي - شارع النصر",
                License = "مشهرة برقم 5678 لسنة 2018"
            };
            await db.Charities.AddAsync(c1);
        }
        else
        {
            c1.Name = "جمعية الأمل لرعاية الأيتام";
            c1.RoleDesc = "كفالة الأيتام والإعانات النقدية والغذائية وتجهيز العرائس";
            c1.Color = "#1565c0";
            c1.Icon = "🤝";
            c1.Phone = "01012345678";
            c1.Address = "القاهرة، المعادي - شارع النصر";
            c1.License = "مشهرة برقم 5678 لسنة 2018";
            db.Charities.Update(c1);
        }

        var c2 = await db.Charities.FirstOrDefaultAsync(c => c.Id == "c2");
        if (c2 == null)
        {
            c2 = new Charity.Domain.Entities.Charity
            {
                Id = "c2",
                Name = "جمعية رسالة الخير والتكافل",
                RoleDesc = "رعاية طبية وقوافل علاجية وكسوة شتاء وإطعام",
                Color = "#2e7d32",
                Icon = "🌿",
                Phone = "01233445566",
                Address = "الجيزة، الدقي - شارع التحرير",
                License = "مشهرة برقم 9012 لسنة 2020"
            };
            await db.Charities.AddAsync(c2);
        }
        else
        {
            c2.Name = "جمعية رسالة الخير والتكافل";
            c2.RoleDesc = "رعاية طبية وقوافل علاجية وكسوة شتاء وإطعام";
            c2.Color = "#2e7d32";
            c2.Icon = "🌿";
            c2.Phone = "01233445566";
            c2.Address = "الجيزة، الدقي - شارع التحرير";
            c2.License = "مشهرة برقم 9012 لسنة 2020";
            db.Charities.Update(c2);
        }

        await db.SaveChangesAsync();

        // 5. Seed Users for each Charity
        // Manager for Charity 1: amal_manager
        var uAmal = await userManager.FindByNameAsync("amal_manager");
        if (uAmal == null)
        {
            uAmal = new ApplicationUser
            {
                UserName = "amal_manager",
                Email = "amal@charity.org",
                FullName = "أحمد سامي (مدير جمعية الأمل)",
                CharityId = "c1",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(uAmal, "123456");
            await userManager.AddToRoleAsync(uAmal, "CharityManager");
        }
        else
        {
            uAmal.FullName = "أحمد سامي (مدير جمعية الأمل)";
            uAmal.CharityId = "c1";
            uAmal.PasswordHash = userManager.PasswordHasher.HashPassword(uAmal, "123456");
            await userManager.UpdateAsync(uAmal);
        }

        // Also update existing "ahmed" to point to c1 with password "123456" for convenience
        var uAhmed = await userManager.FindByNameAsync("ahmed");
        if (uAhmed != null)
        {
            uAhmed.FullName = "أحمد سمير (جمعية الأمل)";
            uAhmed.CharityId = "c1";
            uAhmed.PasswordHash = userManager.PasswordHasher.HashPassword(uAhmed, "123456");
            await userManager.UpdateAsync(uAhmed);
            if (!await userManager.IsInRoleAsync(uAhmed, "CharityManager"))
                await userManager.AddToRoleAsync(uAhmed, "CharityManager");
        }

        // Manager for Charity 2: resala_manager
        var uResala = await userManager.FindByNameAsync("resala_manager");
        if (uResala == null)
        {
            uResala = new ApplicationUser
            {
                UserName = "resala_manager",
                Email = "resala@charity.org",
                FullName = "محمود كريم (مدير جمعية رسالة)",
                CharityId = "c2",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(uResala, "123456");
            await userManager.AddToRoleAsync(uResala, "CharityManager");
        }
        else
        {
            uResala.FullName = "محمود كريم (مدير جمعية رسالة)";
            uResala.CharityId = "c2";
            uResala.PasswordHash = userManager.PasswordHasher.HashPassword(uResala, "123456");
            await userManager.UpdateAsync(uResala);
        }

        // 6. Seed Inventory Items for both charities
        // Charity 1 Inventory (الأمل)
        var inv1 = new InventoryItem
        {
            Id = "inv_amal_1",
            CharityId = "c1",
            ItemName = "كرتونة مواد غذائية متكاملة",
            Type = "أغذية جافة",
            Kind = "صنف ملموس",
            Quantity = 120,
            Unit = "كرتونة",
            MinAlert = 10,
            Code = "ITM-101"
        };
        var inv2 = new InventoryItem
        {
            Id = "inv_amal_2",
            CharityId = "c1",
            ItemName = "لحوم طازجة مجمدة",
            Type = "لحوم ومجمدات",
            Kind = "صنف ملموس",
            Quantity = 65,
            Unit = "كيلو",
            MinAlert = 15,
            Code = "ITM-102"
        };
        var srv1 = new InventoryItem
        {
            Id = "srv_amal_1",
            CharityId = "c1",
            ItemName = "إعانة مالية نقدية عاجلة",
            Type = "خدمات اجتماعية",
            Kind = "خدمة",
            Quantity = 0,
            Unit = "دفعة نقدية",
            Code = "SRV-101"
        };

        // Charity 2 Inventory (رسالة)
        var inv3 = new InventoryItem
        {
            Id = "inv_resala_1",
            CharityId = "c2",
            ItemName = "بطاطين شتوية مبطنة",
            Type = "ملابس وبطاطين",
            Kind = "صنف ملموس",
            Quantity = 90,
            Unit = "بطانية",
            MinAlert = 10,
            Code = "ITM-201"
        };
        var inv4 = new InventoryItem
        {
            Id = "inv_resala_2",
            CharityId = "c2",
            ItemName = "حقيبة أدوية ومستلزمات طبية",
            Type = "أدوية ومستلزمات علاجية",
            Kind = "صنف ملموس",
            Quantity = 45,
            Unit = "حقيبة",
            MinAlert = 5,
            Code = "ITM-202"
        };
        var srv2 = new InventoryItem
        {
            Id = "srv_resala_1",
            CharityId = "c2",
            ItemName = "كشف طبي وقافلة علاجية",
            Type = "خدمات طبية",
            Kind = "خدمة",
            Quantity = 0,
            Unit = "كشف",
            Code = "SRV-201"
        };

        await db.InventoryItems.AddRangeAsync(inv1, inv2, srv1, inv3, inv4, srv2);
        await db.SaveChangesAsync();

        // 7. Seed Beneficiaries
        // Beneficiary 1: Registered by Charity 1 (جمعية الأمل)
        var ben1 = new Beneficiary
        {
            Id = "ben_amal_1",
            FamilyCode = "FAM-26-0001",
            FamilyName = "عائلة محمد محمود حسن",
            NationalId = "28501011234567",
            Name = "محمد محمود حسن",
            Phone = "01098765432",
            Category = "أرامل وأيتام",
            MaritalStatus = "متزوج",
            FamilyCount = 3,
            Address = "القاهرة، المعادي - شارع النصر",
            VillageOrDistrict = "المعادي",
            PriorityTier = "أ - أولوية قصوى ومعدم",
            Status = "مستحق فعال",
            Housing = "إيجار قديم",
            MonthlyIncome = 2200,
            IncomeSource = "عمل يومي غير منتظم",
            RoofType = "خرساني مسلح",
            WallType = "طوب أحمر ومحارة",
            WaterSource = "عداد مياه رسمي",
            SanitationType = "شبكة صرف عمومية",
            ElectricitySource = "عداد رسمي",
            HealthIssues = "ضعف إبصار ومشاكل بالرمد",
            ResearchNotes = "أسرة مستحقة ولديها أطفال بالمدارس وبحاجة لكفالة غذائية دورية.",
            RegisteredByCharityId = "c1",
            RegisteredByCharityName = "جمعية الأمل لرعاية الأيتام",
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        // Beneficiary 2: Registered by Charity 2 (جمعية رسالة الخير)
        var ben2 = new Beneficiary
        {
            Id = "ben_resala_1",
            FamilyCode = "FAM-26-0002",
            FamilyName = "عائلة إبراهيم خليل علي",
            NationalId = "27908051234568",
            Name = "إبراهيم خليل علي",
            Phone = "01122334455",
            Category = "مرضى ومسنين",
            MaritalStatus = "متزوج",
            FamilyCount = 2,
            Address = "الجيزة، الدقي - شارع التحرير",
            VillageOrDistrict = "الدقي",
            PriorityTier = "ب - متوسط الاحتياج",
            Status = "مستحق فعال",
            Housing = "إيجار جديد",
            MonthlyIncome = 1800,
            IncomeSource = "معاش بسيط",
            HasChronicDisease = true,
            ChronicDiseaseDetails = "مرض مزمن بالقلب وضغط",
            NeedsPeriodicMedication = true,
            PeriodicMedicationDetails = "أدوية ضغط وسيولة شهرية",
            MedicationFrequency = "شهري",
            HealthIssues = "مريض قلب ويحتاج متابعة علاجية دورية وقوافل طبية.",
            ResearchNotes = "مسن يعيش مع زوجته وليس لديه عائل، يحتاج دعم علاجي وبطاطين شتاء.",
            RegisteredByCharityId = "c2",
            RegisteredByCharityName = "جمعية رسالة الخير والتكافل",
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await db.Beneficiaries.AddRangeAsync(ben1, ben2);
        await db.SaveChangesAsync();

        // 8. Family Members
        var fm1 = new BeneficiaryFamilyMember
        {
            Id = "fm_amal_1",
            BeneficiaryId = "ben_amal_1",
            NationalId = "28804041234568",
            Name = "فاطمة علي السيد",
            Relationship = "زوجة",
            MemberType = "معال",
            SupportType = "ربة منزل",
            Notes = "ربة منزل ترعى الأبناء"
        };
        var fm2 = new BeneficiaryFamilyMember
        {
            Id = "fm_amal_2",
            BeneficiaryId = "ben_amal_1",
            NationalId = "31502021234569",
            Name = "يوسف محمد محمود",
            Relationship = "ابن",
            MemberType = "معال",
            SupportType = "طالب",
            EducationStage = "ابتدائي",
            Notes = "طالب في الصف الخامس الابتدائي"
        };
        var fm3 = new BeneficiaryFamilyMember
        {
            Id = "fm_resala_1",
            BeneficiaryId = "ben_resala_1",
            NationalId = "28203031234561",
            Name = "زينب كمال عبد الرحيم",
            Relationship = "زوجة",
            MemberType = "معال",
            SupportType = "ربة منزل",
            Notes = "زوجة مسنة مريضة سكر"
        };

        await db.BeneficiaryFamilyMembers.AddRangeAsync(fm1, fm2, fm3);
        await db.SaveChangesAsync();

        // 9. Distributions demonstrating federation & cross-charity history:
        // Movement 1: Amal dispensed a Food Box to its own beneficiary (محمد محمود)
        var dist1 = new Distribution
        {
            Id = "dist_amal_to_ben1",
            BeneficiaryId = "ben_amal_1",
            CharityId = "c1",
            CharityName = "جمعية الأمل لرعاية الأيتام",
            InventoryItemId = "inv_amal_1",
            ItemName = "كرتونة مواد غذائية متكاملة",
            Kind = "صنف ملموس",
            Quantity = 1,
            Unit = "كرتونة",
            Amount = 350,
            Date = "2026-10-01",
            Notes = "صرف كرتونة الدعم التمويني الشهري من جمعية الأمل"
        };

        // Movement 2 (CROSS-CHARITY): Resala dispensed 2 Blankets to Amal's beneficiary (محمد محمود)
        var dist2 = new Distribution
        {
            Id = "dist_resala_to_ben1",
            BeneficiaryId = "ben_amal_1",
            CharityId = "c2",
            CharityName = "جمعية رسالة الخير والتكافل",
            InventoryItemId = "inv_resala_1",
            ItemName = "بطاطين شتوية مبطنة",
            Kind = "صنف ملموس",
            Quantity = 2,
            Unit = "بطانية",
            Amount = 400,
            Date = "2026-10-05",
            Notes = "صرف بطانيتين كسوة شتاء من جمعية رسالة ضمن الحملة المشتركة"
        };

        // Movement 3: Resala dispensed Medical Kit to its own beneficiary (إبراهيم خليل)
        var dist3 = new Distribution
        {
            Id = "dist_resala_to_ben2",
            BeneficiaryId = "ben_resala_1",
            CharityId = "c2",
            CharityName = "جمعية رسالة الخير والتكافل",
            InventoryItemId = "inv_resala_2",
            ItemName = "حقيبة أدوية ومستلزمات طبية",
            Kind = "صنف ملموس",
            Quantity = 1,
            Unit = "حقيبة",
            Amount = 500,
            Date = "2026-10-06",
            Notes = "صرف علاج الضغط والسيولة الشهري من جمعية رسالة"
        };

        await db.Distributions.AddRangeAsync(dist1, dist2, dist3);
        await db.SaveChangesAsync();
    }
}
