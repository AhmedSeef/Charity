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

        // 2c. Seed Governorates (Lookups - المحافظات)
        if (!await context.Governorates.AnyAsync())
        {
            var govs = new List<Governorate>
            {
                new() { Id = "gov_kfs", Name = "كفر الشيخ", NameEn = "Kafr El-Sheikh", IsActive = true },
                new() { Id = "gov_cairo", Name = "القاهرة", NameEn = "Cairo", IsActive = true },
                new() { Id = "gov_giza", Name = "الجيزة", NameEn = "Giza", IsActive = true },
                new() { Id = "gov_alex", Name = "الإسكندرية", NameEn = "Alexandria", IsActive = true },
                new() { Id = "gov_dakahlia", Name = "الدقهلية", NameEn = "Dakahlia", IsActive = true },
                new() { Id = "gov_sharqia", Name = "الشرقية", NameEn = "Sharqia", IsActive = true },
                new() { Id = "gov_gharbia", Name = "الغربية", NameEn = "Gharbia", IsActive = true },
                new() { Id = "gov_beheira", Name = "البحيرة", NameEn = "Beheira", IsActive = true }
            };

            await context.Governorates.AddRangeAsync(govs);
            await context.SaveChangesAsync();
        }

        // 2d. Seed Districts (Lookups - المراكز والأحياء التابعة للمحافظات)
        if (!await context.Districts.AnyAsync())
        {
            var districts = new List<District>
            {
                // مراكز محافظة كفر الشيخ (البداية بمركز مطوبس كما طُلب)
                new() { Id = "dist_motobas", GovernorateId = "gov_kfs", Name = "مركز مطوبس", NameEn = "Motobas District", IsActive = true },
                new() { Id = "dist_desouk", GovernorateId = "gov_kfs", Name = "مركز دسوق", NameEn = "Desouk District", IsActive = true },
                new() { Id = "dist_fouh", GovernorateId = "gov_kfs", Name = "مركز فوه", NameEn = "Fouh District", IsActive = true },
                new() { Id = "dist_kfs", GovernorateId = "gov_kfs", Name = "مركز كفر الشيخ", NameEn = "Kafr El-Sheikh District", IsActive = true },
                new() { Id = "dist_sidi_salem", GovernorateId = "gov_kfs", Name = "مركز سيدي سالم", NameEn = "Sidi Salem District", IsActive = true },
                new() { Id = "dist_baltim", GovernorateId = "gov_kfs", Name = "مركز بلطيم", NameEn = "Baltim District", IsActive = true },
                new() { Id = "dist_bila", GovernorateId = "gov_kfs", Name = "مركز بيلا", NameEn = "Bila District", IsActive = true },
                new() { Id = "dist_hamool", GovernorateId = "gov_kfs", Name = "مركز الحامول", NameEn = "Al-Hamool District", IsActive = true },
                new() { Id = "dist_qallin", GovernorateId = "gov_kfs", Name = "مركز قلين", NameEn = "Qallin District", IsActive = true },
                new() { Id = "dist_riyad", GovernorateId = "gov_kfs", Name = "مركز الرياض", NameEn = "Al-Riyad District", IsActive = true },

                // أحياء ومراكز الجيزة
                new() { Id = "dist_omraniya", GovernorateId = "gov_giza", Name = "حي العمرانية", NameEn = "Al-Omraniya", IsActive = true },
                new() { Id = "dist_safat", GovernorateId = "gov_giza", Name = "صفط اللبن", NameEn = "Safat El-Laban", IsActive = true },
                new() { Id = "dist_dokki", GovernorateId = "gov_giza", Name = "حي الدقي", NameEn = "Al-Dokki", IsActive = true },
                new() { Id = "dist_haram", GovernorateId = "gov_giza", Name = "حي الهرم", NameEn = "Al-Haram", IsActive = true },

                // أحياء ومراكز القاهرة
                new() { Id = "dist_maadi", GovernorateId = "gov_cairo", Name = "حي المعادي", NameEn = "Al-Maadi", IsActive = true },
                new() { Id = "dist_matariya", GovernorateId = "gov_cairo", Name = "حي المطرية", NameEn = "Al-Matariya", IsActive = true },
                new() { Id = "dist_dar_salam", GovernorateId = "gov_cairo", Name = "حي دار السلام", NameEn = "Dar El-Salam", IsActive = true },
                new() { Id = "dist_helwan", GovernorateId = "gov_cairo", Name = "حي حلوان", NameEn = "Helwan", IsActive = true },

                // أحياء الإسكندرية
                new() { Id = "dist_asafra", GovernorateId = "gov_alex", Name = "حي العصافرة", NameEn = "Al-Asafra", IsActive = true },
                new() { Id = "dist_montazah", GovernorateId = "gov_alex", Name = "حي المنتزه", NameEn = "Al-Montazah", IsActive = true },
                new() { Id = "dist_smouha", GovernorateId = "gov_alex", Name = "حي سموحة", NameEn = "Smouha", IsActive = true }
            };

            await context.Districts.AddRangeAsync(districts);
            await context.SaveChangesAsync();
        }

        // 2d-1. Seed Villages (القرى التابعة للمراكز والأحياء - التركيز الأساسي على مركز مطوبس)
        if (!await context.Villages.AnyAsync())
        {
            var villages = new List<Village>
            {
                new() { Id = "vil_monyat_morshid", DistrictId = "dist_motobas", Name = "منية المرشد", NameEn = "Monyat El-Morshid", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_jazeera_khadra", DistrictId = "dist_motobas", Name = "الجزيرة الخضراء", NameEn = "Al-Jazeera Al-Khadra", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_borg_meghizel", DistrictId = "dist_motobas", Name = "برج مغيزل", NameEn = "Borg Meghizel", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_bernbal", DistrictId = "dist_motobas", Name = "برمبال", NameEn = "Bernbal", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_ebiana", DistrictId = "dist_motobas", Name = "إبيانة", NameEn = "Ebiana", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_bany_bakkar", DistrictId = "dist_motobas", Name = "بني بكار", NameEn = "Bany Bakkar", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_basrat", DistrictId = "dist_motobas", Name = "البصراط", NameEn = "Al-Basrat", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_qany", DistrictId = "dist_motobas", Name = "القني", NameEn = "Al-Qany", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_sakarana", DistrictId = "dist_motobas", Name = "السكرانة", NameEn = "Al-Sakarana", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_khalij_bahri", DistrictId = "dist_motobas", Name = "خليج بحري", NameEn = "Khalij Bahri", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_ezbet_amr", DistrictId = "dist_motobas", Name = "عزبة عمرو", NameEn = "Ezbet Amr", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "vil_motobas_city", DistrictId = "dist_motobas", Name = "مدينة مطوبس", NameEn = "Motobas City", IsActive = true, CharityName = "النظام العام" }
            };

            await context.Villages.AddRangeAsync(villages);
            await context.SaveChangesAsync();
        }

        // 2d-2. Seed VillageAreas (المناطق والأحياء التابعة للقرى - تحديداً قرية منية المرشد)
        if (!await context.VillageAreas.AnyAsync())
        {
            var areas = new List<VillageArea>
            {
                new() { Id = "area_manshar", VillageId = "vil_monyat_morshid", Name = "المنشر", NameEn = "Al-Manshar", Notes = "منطقة المنشر - قرية منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_souq", VillageId = "vil_monyat_morshid", Name = "السوق", NameEn = "Al-Souq", Notes = "منطقة السوق التجاري والقديم - قرية منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_jesr", VillageId = "vil_monyat_morshid", Name = "الجسر", NameEn = "Al-Jesr", Notes = "منطقة الجسر وطريق البحر - قرية منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_madreb", VillageId = "vil_monyat_morshid", Name = "المضرب", NameEn = "Al-Madreb", Notes = "منطقة المضرب - قرية منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_torab", VillageId = "vil_monyat_morshid", Name = "الترب", NameEn = "Al-Torab", Notes = "منطقة الترب والمقابر - قرية منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_dair_nahya", VillageId = "vil_monyat_morshid", Name = "داير الناحية", NameEn = "Dayir Al-Nahya", Notes = "وسط البلد وداير الناحية - منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_sharq", VillageId = "vil_monyat_morshid", Name = "المنطقة الشرقية", NameEn = "Eastern Area", Notes = "شرق البلد - منية المرشد", IsActive = true, CharityName = "النظام العام" },
                new() { Id = "area_gharb", VillageId = "vil_monyat_morshid", Name = "المنطقة الغربية", NameEn = "Western Area", Notes = "غرب البلد - منية المرشد", IsActive = true, CharityName = "النظام العام" }
            };

            await context.VillageAreas.AddRangeAsync(areas);
            await context.SaveChangesAsync();
        }

        // 2e. Seed AddressRecords (دليل العناوين والقرى المستقل مع النورماليزيشن)
        if (!await context.AddressRecords.AnyAsync())
        {
            var addresses = new List<(string id, string govId, string distId, string text, int usage)>
            {
                // قرى ومناطق مركز مطوبس (كفر الشيخ)
                ("addr_motobas_1", "gov_kfs", "dist_motobas", "قرية الجزيرة الخضراء", 12),
                ("addr_motobas_2", "gov_kfs", "dist_motobas", "قرية برج مغيزل", 10),
                ("addr_motobas_3", "gov_kfs", "dist_motobas", "قرية معدية مهدي", 8),
                ("addr_motobas_4", "gov_kfs", "dist_motobas", "قرية برنبال (برمبال)", 15),
                ("addr_motobas_5", "gov_kfs", "dist_motobas", "قرية منية المرشد", 9),
                ("addr_motobas_6", "gov_kfs", "dist_motobas", "قرية إبيانة", 7),
                ("addr_motobas_7", "gov_kfs", "dist_motobas", "قرية البصراط", 6),
                ("addr_motobas_8", "gov_kfs", "dist_motobas", "قرية بني بكار", 5),
                ("addr_motobas_9", "gov_kfs", "dist_motobas", "قرية القني", 4),
                ("addr_motobas_10", "gov_kfs", "dist_motobas", "قرية خليج بحري", 8),
                ("addr_motobas_11", "gov_kfs", "dist_motobas", "قرية السكرانة", 6),
                ("addr_motobas_12", "gov_kfs", "dist_motobas", "عزبة عمرو", 5),
                ("addr_motobas_13", "gov_kfs", "dist_motobas", "شارع داير الناحية - مطوبس", 14),
                ("addr_motobas_14", "gov_kfs", "dist_motobas", "مساكن المحكمة - مطوبس", 11),

                // قرى ومناطق مركز دسوق (كفر الشيخ)
                ("addr_desouk_1", "gov_kfs", "dist_desouk", "قرية شباس الشهداء", 9),
                ("addr_desouk_2", "gov_kfs", "dist_desouk", "قرية سنهور المدينة", 8),
                ("addr_desouk_3", "gov_kfs", "dist_desouk", "قرية محلة دياي", 6),
                ("addr_desouk_4", "gov_kfs", "dist_desouk", "شارع الجيش - دسوق", 10),

                // قرى ومناطق مركز فوه (كفر الشيخ)
                ("addr_fouh_1", "gov_kfs", "dist_fouh", "قرية سنديون", 7),
                ("addr_fouh_2", "gov_kfs", "dist_fouh", "قرية قبريط", 6),
                ("addr_fouh_3", "gov_kfs", "dist_fouh", "شارع الكورنيش - فوه", 8),

                // مناطق شائعة بالجيزة والقاهرة والإسكندرية
                ("addr_omraniya_1", "gov_giza", "dist_omraniya", "العمرانية الغربية، شارع ترعة الزمر", 18),
                ("addr_omraniya_2", "gov_giza", "dist_omraniya", "العمرانية الشرقية، شارع عثمان محرم", 14),
                ("addr_safat_1", "gov_giza", "dist_safat", "صفط اللبن، شارع التحرير وموقف الميكروباص", 16),
                ("addr_maadi_1", "gov_cairo", "dist_maadi", "طرة المعادي، شارع كورنيش النيل", 15),
                ("addr_matariya_1", "gov_cairo", "dist_matariya", "المطرية، عزبة النخل، شارع التروللي", 12),
                ("addr_asafra_1", "gov_alex", "dist_asafra", "العصافرة قبلي، شارع المعهد الديني", 14),
                ("addr_montazah_1", "gov_alex", "dist_montazah", "المنتزه، سيدي بشر قبلي، شارع جمال عبد الناصر", 16)
            };

            var addressEntities = addresses.Select(a => new AddressRecord
            {
                Id = a.id,
                GovernorateId = a.govId,
                DistrictId = a.distId,
                AddressText = a.text,
                NormalizedText = Charity.Application.Services.Implementations.AddressNormalizationService.Normalize(a.text),
                UsageCount = a.usage,
                CharityName = "النظام العام"
            }).ToList();

            await context.AddressRecords.AddRangeAsync(addressEntities);
            await context.SaveChangesAsync();
        }

        // 2e. Seed Universal Expense Categories (Lookups - أنواع وبنود المصروفات العامة)
        if (!await context.ExpenseCategories.AnyAsync())
        {
            var expCategories = new List<ExpenseCategory>
            {
                new()
                {
                    Id = "exp_medical",
                    Code = "EXP-001",
                    Name = "إعانات علاجية وعمليات جراحية",
                    Description = "تغطية تكاليف الفحوصات الطبية، العمليات الجراحية، وشراء الأدوية المزمنة لغير القادرين.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                },
                new()
                {
                    Id = "exp_monthly",
                    Code = "EXP-002",
                    Name = "مساعدات نقدية شهرية دورية",
                    Description = "مبالغ نقدية منتظمة تكفل الأسر المعدمة والأرامل والأيتام الذين لا عائل لهم ولا دخل ثابت.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                },
                new()
                {
                    Id = "exp_marriage",
                    Code = "EXP-003",
                    Name = "تجهيز فتيات مقبلات على الزواج وأيتام",
                    Description = "مساهمات نقدية لشراء الأجهزة الكهربائية ومستلزمات الزواج الأساسية للفتيات اليتيمات.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                },
                new()
                {
                    Id = "exp_education",
                    Code = "EXP-004",
                    Name = "مصاريف ومساعدات تعليمية",
                    Description = "سداد المصروفات المدرسية والجامعية، وتوفير الكتب والزي المدرسي للطلاب غير القادرين.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                },
                new()
                {
                    Id = "exp_debt",
                    Code = "EXP-005",
                    Name = "سداد ديون وغارمين",
                    Description = "سداد الديون وإيصالات الأمانة للحالات المهددة بالسجن بعد بحث اجتماعي وقانوني معتمد.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                },
                new()
                {
                    Id = "exp_emergency",
                    Code = "EXP-006",
                    Name = "إعانات طارئة وظروف استثنائية",
                    Description = "دعم مالي فوري لحالات الحوادث والوفاة والظروف الطارئة المفاجئة التي تصيب الأسر.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                },
                new()
                {
                    Id = "exp_housing",
                    Code = "EXP-007",
                    Name = "تسقيف وترميم منازل وتأهيل مأوى",
                    Description = "مساهمات نقدية لشراء مواد البناء والصب وألواح السقف لستر بيوت الأسر الفقيرة.",
                    IsActive = true,
                    CharityId = null,
                    CharityName = "النظام العام"
                }
            };

            await context.ExpenseCategories.AddRangeAsync(expCategories);
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
        await TenantDefaults.EnsureAsync(context);
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

        // 2f. Seed Motobas / Monyat El-Morshid Beneficiaries
        if (!await context.Beneficiaries.AnyAsync(b => b.DistrictId == "dist_motobas"))
        {
            var motobasBeneficiaries = new List<Beneficiary>
            {
                new()
                {
                    Id = "ben_motobas_1",
                    FamilyCode = "FAM-MOT-01",
                    FamilyName = "عائلة السيد عبد المجيد",
                    NationalId = "28501011812341",
                    Name = "السيد عبد المجيد إبراهيم",
                    Category = "أرامل وأيتام",
                    GovernorateId = "gov_kfs",
                    GovernorateName = "كفر الشيخ",
                    DistrictId = "dist_motobas",
                    DistrictName = "مركز مطوبس",
                    VillageId = "vil_monyat_morshid",
                    VillageName = "منية المرشد",
                    VillageAreaId = "area_manshar",
                    VillageAreaName = "المنشر",
                    Address = "منية المرشد، منطقة المنشر، بجوار مسجد المنشر",
                    VillageOrDistrict = "المنشر - منية المرشد",
                    PriorityTier = "أ - أولوية قصوى ومعدم",
                    Status = "مستحق فعال",
                    Phone = "01011122233",
                    MaritalStatus = "متزوج",
                    FamilyCount = 5,
                    Housing = "ملك ريفي قديم",
                    MonthlyIncome = 1200,
                    IncomeSource = "يومية عمالة زراعية غير منتظمة",
                    RoofType = "جريد وقش وبوص",
                    WallType = "طوب أحمر ومحارة",
                    WaterSource = "طلمبة حبشية",
                    SanitationType = "بيارة/طرنش",
                    ElectricitySource = "عداد كودي/ممارسة",
                    ResearchNotes = "أسرة معدمة في منطقة المنشر بمنية المرشد، السقف خشب وبوص بحاجة لتسقيف وبطاطين ووجبات غذائية.",
                    RegisteredByCharityId = "c3",
                    RegisteredByCharityName = "جمعية رسالة الصداقة"
                },
                new()
                {
                    Id = "ben_motobas_2",
                    FamilyCode = "FAM-MOT-02",
                    FamilyName = "عائلة فتحي مصطفى عامر",
                    NationalId = "27902021812342",
                    Name = "فتحي مصطفى عامر",
                    Category = "مرضى ومسنين",
                    GovernorateId = "gov_kfs",
                    GovernorateName = "كفر الشيخ",
                    DistrictId = "dist_motobas",
                    DistrictName = "مركز مطوبس",
                    VillageId = "vil_monyat_morshid",
                    VillageName = "منية المرشد",
                    VillageAreaId = "area_souq",
                    VillageAreaName = "السوق",
                    Address = "منية المرشد، منطقة السوق، حارة العمدة",
                    VillageOrDistrict = "السوق - منية المرشد",
                    PriorityTier = "أ - أولوية قصوى ومعدم",
                    Status = "مستحق فعال",
                    Phone = "01122233344",
                    MaritalStatus = "متزوج",
                    FamilyCount = 3,
                    Housing = "إيجار بسيط",
                    MonthlyIncome = 900,
                    IncomeSource = "لا يعمل لكبر السن وعجز طبي",
                    HasChronicDisease = true,
                    ChronicDiseaseDetails = "شلل جزئي وجلطة",
                    NeedsPeriodicMedication = true,
                    PeriodicMedicationDetails = "علاج أعصاب وسيولة",
                    MedicationFrequency = "شهري",
                    ResearchNotes = "مسن طريح الفراش في منطقة السوق القديم بمنية المرشد يحتاج كرتونة طعام وعلاج شهري.",
                    RegisteredByCharityId = "c3",
                    RegisteredByCharityName = "جمعية رسالة الصداقة"
                },
                new()
                {
                    Id = "ben_motobas_3",
                    FamilyCode = "FAM-MOT-03",
                    FamilyName = "عائلة أمينة حسن الدسوقي",
                    NationalId = "29103031812343",
                    Name = "أمينة حسن الدسوقي",
                    Category = "أرامل وأيتام",
                    GovernorateId = "gov_kfs",
                    GovernorateName = "كفر الشيخ",
                    DistrictId = "dist_motobas",
                    DistrictName = "مركز مطوبس",
                    VillageId = "vil_monyat_morshid",
                    VillageName = "منية المرشد",
                    VillageAreaId = "area_jesr",
                    VillageAreaName = "الجسر",
                    Address = "منية المرشد، منطقة الجسر، طريق البحر",
                    VillageOrDistrict = "الجسر - منية المرشد",
                    PriorityTier = "أ - أولوية قصوى ومعدم",
                    Status = "مستحق فعال",
                    Phone = "01233344455",
                    MaritalStatus = "أرمل",
                    FamilyCount = 4,
                    Housing = "سكن متهالك",
                    MonthlyIncome = 850,
                    IncomeSource = "تكافل وكرامة",
                    HasGovernmentPension = true,
                    GovernmentPensionDetails = "معاش تكافل وكرامة 850 ج",
                    RoofType = "خشب وألواح",
                    ResearchNotes = "أرملة تعول 3 أيتام على طريق الجسر بمنية المرشد.",
                    RegisteredByCharityId = "c3",
                    RegisteredByCharityName = "جمعية رسالة الصداقة"
                },
                new()
                {
                    Id = "ben_motobas_4",
                    FamilyCode = "FAM-MOT-04",
                    FamilyName = "عائلة سعد راغب بدوي",
                    NationalId = "28304041812344",
                    Name = "سعد راغب بدوي",
                    Category = "حالات طوارئ",
                    GovernorateId = "gov_kfs",
                    GovernorateName = "كفر الشيخ",
                    DistrictId = "dist_motobas",
                    DistrictName = "مركز مطوبس",
                    VillageId = "vil_monyat_morshid",
                    VillageName = "منية المرشد",
                    VillageAreaId = "area_madreb",
                    VillageAreaName = "المضرب",
                    Address = "منية المرشد، منطقة المضرب، خلف مضرب الأرز",
                    VillageOrDistrict = "المضرب - منية المرشد",
                    PriorityTier = "ب - متوسط الاحتياج",
                    Status = "مستحق فعال",
                    Phone = "01544455566",
                    MaritalStatus = "متزوج",
                    FamilyCount = 6,
                    Housing = "ملك ريفي",
                    MonthlyIncome = 2200,
                    IncomeSource = "عامل زراعي وصياد",
                    ResearchNotes = "أسرة في منطقة المضرب تحتاج دعم غذائي دوري وحقائب مدرسية.",
                    RegisteredByCharityId = "c3",
                    RegisteredByCharityName = "جمعية رسالة الصداقة"
                },
                new()
                {
                    Id = "ben_motobas_5",
                    FamilyCode = "FAM-MOT-05",
                    FamilyName = "عائلة خضرة عبد العال",
                    NationalId = "26805051812345",
                    Name = "خضرة عبد العال مصطفى",
                    Category = "أرامل وأيتام",
                    GovernorateId = "gov_kfs",
                    GovernorateName = "كفر الشيخ",
                    DistrictId = "dist_motobas",
                    DistrictName = "مركز مطوبس",
                    VillageId = "vil_monyat_morshid",
                    VillageName = "منية المرشد",
                    VillageAreaId = "area_torab",
                    VillageAreaName = "الترب",
                    Address = "منية المرشد، منطقة الترب، بجوار المقابر الشرقية",
                    VillageOrDistrict = "الترب - منية المرشد",
                    PriorityTier = "أ - أولوية قصوى ومعدم",
                    Status = "مستحق فعال",
                    Phone = "01055566677",
                    MaritalStatus = "أرمل",
                    FamilyCount = 2,
                    Housing = "غرفة ملحقة",
                    MonthlyIncome = 700,
                    IncomeSource = "مساعدات الأهالي",
                    RoofType = "جريد وقش وبوص",
                    ResearchNotes = "مسنة وأرملة في منطقة الترب بدون معيل وبحاجة ماسة للطعام والعلاج.",
                    RegisteredByCharityId = "c3",
                    RegisteredByCharityName = "جمعية رسالة الصداقة"
                }
            };

            await context.Beneficiaries.AddRangeAsync(motobasBeneficiaries);
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
        await TenantDefaults.EnsureAsync(context);
    }
}
