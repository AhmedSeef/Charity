using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Charity.Domain.Entities;
using Charity.Infrastructure.Data.Context;

namespace Charity.Infrastructure.Data.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(CharityDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles (Lookups)
        string[] roles = ["Admin", "CharityManager", "Staff"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Universal Suspension Reasons (Lookups - أسباب الإيقاف العامة بالشبكة)
        if (!await context.SuspensionReasons.AnyAsync())
        {
            var reasons = new List<SuspensionReason>
            {
                new() { Id = "sr1", Title = "وفاة صاحب الحالة", Description = "وفاة المستفيد المسجل بالملف وانتقال الرعاية أو إغلاق الملف", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sr2", Title = "تحسن الحالة المادية ووجود دخل كافٍ", Description = "تحسن الوضع الاقتصادي للأسرة وتوفر مصدر دخل ثابت ومستقر", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sr3", Title = "تغيير محل الإقامة خارج النطاق الجغرافي", Description = "انتقال الأسرة لمحافظة أو منطقة سكنية خارج النطاق الجغرافي للجمعية", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sr4", Title = "ازدواجية الصرف والاستفادة من جهات أخرى", Description = "ثبوت استلام مساعدات متكررة وكافية من جهات أو جمعيات شريكة أخرى", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sr5", Title = "زواج الفتاة / استقلال الحالة", Description = "زواج الحالة المستفيدة أو استقلالها بأسرة جديدة", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sr6", Title = "عدم صحة البيانات المقدمة في البحث", Description = "اكتشاف تضارب أو عدم دقة في بيانات الدخل أو الممتلكات", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sr7", Title = "عدم التجاوب مع البحث الميداني الدوري", Description = "تعذر التواصل مع الحالة ورفض استقبال الباحث الاجتماعي لإعادة البحث", IsActive = true, CharityId = null, CharityName = "النظام العام" }
            };

            await context.SuspensionReasons.AddRangeAsync(reasons);
            await context.SaveChangesAsync();
        }

        // 2b. Seed Universal Separation Reasons (Lookups - أسباب انفصال الأفراد العامة)
        if (!await context.SeparationReasons.AnyAsync())
        {
            var sepReasons = new List<SeparationReason>
            {
                new() { Id = "sep1", Title = "استقلال بمسكن خاص وزواج", Description = "استقلال الفرد بعد الزواج وتأسيس مسكن ومعيشة منفصلة", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sep2", Title = "تكوين أسرة جديدة مستحقة", Description = "تكوين أسرة جديدة تتطلب ملفاً مستقلاً وتقديم دعم اجتماعي خاص بها", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sep3", Title = "بلوغ سن الرشد والعمل", Description = "بلوغ السن القانوني واكتساب دخل مستقل وتحمل نفقاته الشخصية", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sep4", Title = "الانتقال لمحافظة أخرى", Description = "انتقال إقامة الفرد بشكل دائم خارج نطاق المحافظة ومسكن الأسرة", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sep5", Title = "فك ارتباط بملف الأسرة فقط", Description = "استبعاد الفرد من قائمة المعالين دون الرغبة في فتح ملف مساعدات جديد", IsActive = true, CharityId = null, CharityName = "النظام العام" },
                new() { Id = "sep6", Title = "انفصال عائلي وطلاق", Description = "انفصال الزوجة بعد الطلاق واستقلال الحضانة والمعيشة في مسكن خاص", IsActive = true, CharityId = null, CharityName = "النظام العام" }
            };

            await context.SeparationReasons.AddRangeAsync(sepReasons);
            await context.SaveChangesAsync();
        }

        // 3. Seed ONLY the platform super administrator ("كبير مسؤولي المنصة")
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@charity.org",
                FullName = "مدير شبكة الجمعيات",
                CharityId = null,
                EmailConfirmed = true
            };

            var createRes = await userManager.CreateAsync(superAdmin);
            if (!createRes.Succeeded)
            {
                throw new InvalidOperationException($"Could not create admin: {string.Join(", ", createRes.Errors.Select(e => e.Description))}");
            }
            superAdmin.PasswordHash = userManager.PasswordHasher.HashPassword(superAdmin, "admin");
            superAdmin.SecurityStamp = Guid.NewGuid().ToString();
            await userManager.UpdateAsync(superAdmin);
            await userManager.AddToRoleAsync(superAdmin, "Admin");
        }
        else
        {
            if (string.IsNullOrEmpty(adminUser.FullName))
            {
                adminUser.FullName = "مدير شبكة الجمعيات";
            }
            if (await userManager.CheckPasswordAsync(adminUser, "Admin@123"))
            {
                adminUser.PasswordHash = userManager.PasswordHasher.HashPassword(adminUser, "admin");
                adminUser.SecurityStamp = Guid.NewGuid().ToString();
            }
            await userManager.UpdateAsync(adminUser);

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    /// <summary>
    /// Seeds disposable test fixtures used exclusively by integration tests (e.g., CharityApiFactory).
    /// The production/application database never calls this method.
    /// </summary>
    public static async Task SeedTestFixturesAsync(CharityDbContext context)
    {
        // 1. Seed Test Charities
        if (!await context.Charities.AnyAsync())
        {
            var charities = new List<Charity.Domain.Entities.Charity>
            {
                new()
                {
                    Id = "c1",
                    Name = "جمعية أبواب الخير",
                    RoleDesc = "رعاية عامة وإطعام ومساعدات للأسر الأكثر احتياجاً",
                    Logo = "",
                    Color = "#004d40",
                    Icon = "🏢",
                    Phone = "01012345678",
                    Address = "الجيزة، العمرانية",
                    License = "مشهرة برقم 1234 لسنة 2015"
                },
                new()
                {
                    Id = "c2",
                    Name = "جمعية الأمل لرعاية الأيتام",
                    RoleDesc = "كفالة الأيتام والإعانات النقدية وتجهيز العرائس",
                    Logo = "",
                    Color = "#1565c0",
                    Icon = "🤝",
                    Phone = "01122334455",
                    Address = "القاهرة، المعادي",
                    License = "مشهرة برقم 5678 لسنة 2018"
                },
                new()
                {
                    Id = "c3",
                    Name = "جمعية رسالة الصداقة",
                    RoleDesc = "رعاية طبية وقوافل علاجية وكراسي متحركة",
                    Logo = "",
                    Color = "#6a1b9a",
                    Icon = "🏥",
                    Phone = "01233445566",
                    Address = "الإسكندرية، سموحة",
                    License = "مشهرة برقم 9012 لسنة 2020"
                }
            };

            await context.Charities.AddRangeAsync(charities);
            await context.SaveChangesAsync();
        }

        // 2. Seed Test Beneficiaries
        if (!await context.Beneficiaries.AnyAsync())
        {
            var beneficiaries = new List<Beneficiary>
            {
                new()
                {
                    Id = "b1",
                    FamilyCode = "FAM-26-0001",
                    FamilyName = "عائلة أحمد محمود سالم",
                    NationalId = "28501011234567",
                    Name = "أحمد محمود سالم",
                    Category = "أرامل وأيتام",
                    Address = "الجيزة، العمرانية",
                    VillageOrDistrict = "العمرانية الغربية",
                    PriorityTier = "أ - أولوية قصوى ومعدم",
                    Status = "مستحق فعال",
                    Phone = "01098765432",
                    MaritalStatus = "متزوج",
                    FamilyCount = 4,
                    Housing = "إيجار قديم",
                    MonthlyIncome = 2500,
                    IncomeSource = "أعمال يومية غير منتظمة",
                    RoofType = "خرساني مسلح",
                    WallType = "طوب أحمر ومحارة",
                    WaterSource = "عداد مياه رسمي",
                    SanitationType = "شبكة صرف صحي عمومية",
                    ElectricitySource = "عداد قانوني",
                    HealthIssues = "ضعف إبصار ومشاكل بالرمد",
                    ResearchNotes = "أسرة تعول 3 أطفال، عائل الأسرة باليومية وبحاجة لمساعدات دورية وكفالة تعليمية.",
                    RegisteredByCharityId = "c1",
                    RegisteredByCharityName = "جمعية أبواب الخير"
                },
                new()
                {
                    Id = "b2",
                    FamilyCode = "FAM-26-0002",
                    FamilyName = "عائلة سعاد إبراهيم محمد",
                    NationalId = "27005057654321",
                    Name = "سعاد إبراهيم محمد",
                    Category = "مرضى ومسنين",
                    Address = "القاهرة، المطرية",
                    VillageOrDistrict = "المطرية",
                    PriorityTier = "ب - متوسط الاحتياج",
                    Status = "مستحق فعال",
                    Phone = "01187654321",
                    MaritalStatus = "أرمل",
                    FamilyCount = 2,
                    Housing = "سكن مشترك",
                    MonthlyIncome = 1800,
                    IncomeSource = "مساعدة عائلية بسيطة",
                    HasChronicDisease = true,
                    ChronicDiseaseDetails = "سكري وضغط مزمن",
                    NeedsPeriodicMedication = true,
                    PeriodicMedicationDetails = "أنسولين وأدوية ضغط",
                    MedicationFrequency = "شهري",
                    HealthIssues = "مريضة سكر وتحتاج أدوية شهرية وكرسي متحرك",
                    ResearchNotes = "مسنة تعيش مع ابنتها وتحتاج إعانة علاجية دورية.",
                    RegisteredByCharityId = "c1",
                    RegisteredByCharityName = "جمعية أبواب الخير"
                },
                new()
                {
                    Id = "b3",
                    FamilyCode = "FAM-26-0003",
                    FamilyName = "عائلة محمود حسن علي",
                    NationalId = "29012345678901",
                    Name = "محمود حسن علي",
                    Category = "غارمين",
                    Address = "المعادي، القاهرة",
                    VillageOrDistrict = "طرة المعادي",
                    PriorityTier = "أ - أولوية قصوى ومعدم",
                    Status = "مستحق فعال",
                    Phone = "01234567890",
                    MaritalStatus = "متزوج",
                    FamilyCount = 5,
                    Housing = "إيجار جديد",
                    MonthlyIncome = 3200,
                    IncomeSource = "سائق تاكسي باليومية",
                    HasDebts = true,
                    TotalDebtAmount = 24000,
                    DebtCreditor = "تاجر أجهزة منزلية",
                    IsThreatenedWithImprisonment = true,
                    DebtNotes = "إيصالات أمانة متأخرة لجهاز ابنته الكبرى مهدد بالسجن خلال شهرين.",
                    HealthIssues = "سليم طبياً",
                    ResearchNotes = "عليه ديون متراكمة بسبب جهاز ابنته ومطالب بأقساط عاجلة.",
                    RegisteredByCharityId = "c2",
                    RegisteredByCharityName = "جمعية الأمل لرعاية الأيتام"
                },
                new()
                {
                    Id = "b4",
                    FamilyCode = "FAM-26-0004",
                    FamilyName = "عائلة فاطمة عبد الرحمن",
                    NationalId = "28009091112223",
                    Name = "فاطمة عبد الرحمن",
                    Category = "حالات طوارئ",
                    Address = "الإسكندرية، العصافرة",
                    VillageOrDistrict = "العصافرة قبلي",
                    PriorityTier = "ج - طوارئ وظرف مؤقت",
                    Status = "مستحق مؤقت",
                    Phone = "01511223344",
                    MaritalStatus = "مطلق",
                    FamilyCount = 3,
                    Housing = "سكن متهالك/يحتاج ترميم",
                    MonthlyIncome = 1500,
                    IncomeSource = "نفقة غير منتظمة",
                    RoofType = "صاج وزينك",
                    WallType = "طوب أحمر بدون محارة",
                    WaterSource = "طلمبة حبشية",
                    SanitationType = "بيارة/طرنش",
                    ElectricitySource = "عداد كودي/ممارسة",
                    HealthIssues = "روماتويد والتهاب مفاصل",
                    ResearchNotes = "تحتاج مساعدة شتاء وبطاطين لأطفالها وترميم بسيط للسقف.",
                    RegisteredByCharityId = "c3",
                    RegisteredByCharityName = "جمعية رسالة الصداقة"
                }
            };

            await context.Beneficiaries.AddRangeAsync(beneficiaries);
            await context.SaveChangesAsync();
        }

        // 3. Seed Test Beneficiary Family Members
        if (!await context.BeneficiaryFamilyMembers.AnyAsync())
        {
            var familyMembers = new List<BeneficiaryFamilyMember>
            {
                new()
                {
                    Id = "fm_b1_1",
                    BeneficiaryId = "b1",
                    NationalId = "28804041234568",
                    Name = "مريم كمال السيد",
                    Relationship = "زوجة",
                    MemberType = "معال",
                    HasIncome = false,
                    SupportType = "ربة منزل",
                    Notes = "ربة منزل ترعى الأطفال"
                },
                new()
                {
                    Id = "fm_b1_2",
                    BeneficiaryId = "b1",
                    NationalId = "31502021234569",
                    Name = "يوسف أحمد محمود",
                    Relationship = "ابن",
                    MemberType = "معال",
                    HasIncome = false,
                    SupportType = "طالب",
                    Notes = "طالب بالمرحلة الابتدائية"
                },
                new()
                {
                    Id = "fm_b1_3",
                    BeneficiaryId = "b1",
                    NationalId = "31806061234560",
                    Name = "نور أحمد محمود",
                    Relationship = "ابنة",
                    MemberType = "معال",
                    HasIncome = false,
                    SupportType = "طفل",
                    Notes = "طفلة في سن الحضانة"
                },
                new()
                {
                    Id = "fm_b2_1",
                    BeneficiaryId = "b2",
                    NationalId = "29801017654322",
                    Name = "هناء محمود إسماعيل",
                    Relationship = "ابنة",
                    MemberType = "معيل",
                    HasIncome = true,
                    IncomeAmount = 1500,
                    IncomeSource = "عمل خياطة وتفصيل بالمنزل",
                    SupportType = "أخرى",
                    Notes = "ابنتها وتساعد في نفقات العلاج البسيطة"
                }
            };

            await context.BeneficiaryFamilyMembers.AddRangeAsync(familyMembers);
            await context.SaveChangesAsync();
        }

        // 4. Seed Test Inventory Items
        if (!await context.InventoryItems.AnyAsync())
        {
            var items = new List<InventoryItem>
            {
                new() { Id = "i1", CharityId = "c1", ItemName = "أرز مصري ممتاز", Quantity = 450, Unit = "كيلو", Type = "أغذية جافة", Kind = "صنف ملموس", Code = "ITM-001" },
                new() { Id = "i2", CharityId = "c1", ItemName = "سكر أبيض نقي", Quantity = 300, Unit = "كيلو", Type = "أغذية جافة", Kind = "صنف ملموس", Code = "ITM-002" },
                new() { Id = "i7", CharityId = "c1", ItemName = "زيت طعام ممتاز", Quantity = 95, Unit = "زجاجة", Type = "أغذية جافة", Kind = "صنف ملموس", Code = "ITM-007" },
                new() { Id = "s_abwab_1", CharityId = "c1", ItemName = "سداد ديون غارم/غارمة", Quantity = 0, Unit = "حالة", Type = "خدمات اجتماعية", Kind = "خدمة", Code = "SRV-014" },
                new() { Id = "i_amal_1", CharityId = "c2", ItemName = "بطاطين شتوية مبطنة", Quantity = 180, Unit = "قطعة", Type = "ملابس وبطاطين", Kind = "صنف ملموس", Code = "ITM-201" },
                new() { Id = "i_amal_2", CharityId = "c2", ItemName = "شنطة أدوات مدرسية للأيتام", Quantity = 75, Unit = "شنطة", Type = "متنوع", Kind = "صنف ملموس", Code = "ITM-202" },
                new() { Id = "s_amal_1", CharityId = "c2", ItemName = "إعانة مالية نقدية عاجلة", Quantity = 0, Unit = "دفعة نقدية", Type = "خدمات اجتماعية وتجهيز", Kind = "خدمة", Code = "SRV-201" },
                new() { Id = "i_sedaka_1", CharityId = "c3", ItemName = "كرسي متحرك للمرضى", Quantity = 8, Unit = "قطعة", Type = "أجهزة تعويضية وكراسي", Kind = "صنف ملموس", Code = "ITM-301" },
                new() { Id = "i_sedaka_2", CharityId = "c3", ItemName = "أدوية سكر وضغط", Quantity = 60, Unit = "علبة", Type = "أدوية ومستلزمات طبية", Kind = "صنف ملموس", Code = "ITM-302" },
                new() { Id = "s_sedaka_1", CharityId = "c3", ItemName = "كشف رمد وعيون ونظارة طبية", Quantity = 0, Unit = "كشف ونظارة", Type = "خدمات طبية وعلاجية", Kind = "خدمة", Code = "SRV-301" }
            };

            await context.InventoryItems.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }

        // 5. Seed Test Distributions
        if (!await context.Distributions.AnyAsync())
        {
            var distributions = new List<Distribution>
            {
                new()
                {
                    Id = "dist1",
                    BeneficiaryId = "b1",
                    InventoryItemId = "i1",
                    ItemName = "أرز مصري ممتاز",
                    Kind = "صنف ملموس",
                    Quantity = 5,
                    Unit = "كيلو",
                    Amount = 150,
                    Date = "2026-10-01",
                    Notes = "صرف دعم تمويني شهري",
                    CharityId = "c1",
                    CharityName = "جمعية أبواب الخير"
                },
                new()
                {
                    Id = "dist_amal_user_example",
                    BeneficiaryId = "b1",
                    InventoryItemId = "s_amal_1",
                    ItemName = "إعانة مالية نقدية عاجلة",
                    Kind = "إعانة نقدية",
                    Quantity = 1,
                    Unit = "دفعة نقدية",
                    Amount = 1000,
                    Date = "2026-10-05",
                    Notes = "صرف 1,000 ج.م إعانة نقدية طارئة",
                    CharityId = "c2",
                    CharityName = "جمعية الأمل لرعاية الأيتام"
                },
                new()
                {
                    Id = "dist_sedaka_medical",
                    BeneficiaryId = "b1",
                    InventoryItemId = "s_sedaka_1",
                    ItemName = "كشف رمد وعيون ونظارة طبية",
                    Kind = "خدمة",
                    Quantity = 1,
                    Unit = "كشف ونظارة",
                    Amount = 350,
                    Date = "2026-10-09",
                    Notes = "كشف بالقافلة الطبية وتسليم نظارة قراءة",
                    CharityId = "c3",
                    CharityName = "جمعية رسالة الصداقة"
                }
            };

            await context.Distributions.AddRangeAsync(distributions);
            await context.SaveChangesAsync();
        }

        // 6. Seed Test Staff
        if (!await context.StaffMembers.AnyAsync())
        {
            var staff = new List<StaffMember>
            {
                new() { Id = "s1", CharityId = "c1", Name = "أحمد المتطوع", Phone = "01000000000", Type = "متطوع", Role = "باحث اجتماعي ميداني", JoinDate = "2026-01-01" },
                new() { Id = "s2", CharityId = "c1", Name = "م. طارق عبد الحميد", Phone = "01122334411", Type = "موظف براتب", Role = "أمين المخازن", Salary = 5500, JoinDate = "2025-06-01" },
                new() { Id = "s_amal_1", CharityId = "c2", Name = "د. سمير عبد الله", Phone = "01099881122", Type = "موظف براتب", Role = "مدير دار الأيتام", Salary = 7000, JoinDate = "2024-03-01" },
                new() { Id = "s_sedaka_1", CharityId = "c3", Name = "د. شريف عيون", Phone = "01233441122", Type = "متطوع", Role = "طبيب منسق القوافل", JoinDate = "2025-01-10" }
            };

            await context.StaffMembers.AddRangeAsync(staff);
            await context.SaveChangesAsync();
        }

        await RepresentativeDemoData.SeedAsync(context);
    }
}
