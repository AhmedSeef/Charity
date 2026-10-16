using Charity.Domain.Entities;
using Charity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Charity.Infrastructure.Data.Seed;

/// <summary>
/// Fictional, representative records for a useful first-run demo. Fixed IDs make
/// the seed idempotent; existing records (including user edits) are never updated.
/// </summary>
public static class RepresentativeDemoData
{
    public static async Task SeedAsync(CharityDbContext db)
    {
        var families = new[]
        {
            new Beneficiary { Id = "demo-family-101", FamilyCode = "FAM-26-0101", FamilyName = "أسرة هدى عبد الله", NationalId = "28207150100101", Name = "هدى عبد الله علي", Phone = "01000001001", RegisteredByCharityId = "c1", RegisteredByCharityName = "جمعية أبواب الخير", Category = "أرامل وأيتام", MaritalStatus = "أرمل", FamilyCount = 4, Address = "الجيزة، صفط اللبن", VillageOrDistrict = "صفط اللبن", Housing = "إيجار جديد", MonthlyIncome = 2800, IncomeSource = "خياطة منزلية", PriorityTier = "أ - أولوية قصوى ومعدم", RoofType = "صاج وزينك", WallType = "طوب أحمر", WaterSource = "ملء من الجيران", SanitationType = "بيارة/طرنش", ElectricitySource = "عداد كودي/ممارسة", ResearchNotes = "دخل غير منتظم وثلاثة أطفال بالمدارس؛ تحتاج دعمًا غذائيًا ومتابعة تعليمية.", IsCandidateForMicroProject = true, SuggestedMicroProject = "ماكينة خياطة" },
            new Beneficiary { Id = "demo-family-102", FamilyCode = "FAM-26-0102", FamilyName = "أسرة عبد الرحمن سالم", NationalId = "27503150100102", Name = "عبد الرحمن سالم فتحي", Phone = "01000001002", RegisteredByCharityId = "c1", RegisteredByCharityName = "جمعية أبواب الخير", Category = "مرضى ومسنين", MaritalStatus = "متزوج", FamilyCount = 2, Address = "الجيزة، العمرانية", VillageOrDistrict = "العمرانية", Housing = "إيجار قديم", MonthlyIncome = 1800, IncomeSource = "معاش", HasGovernmentPension = true, GovernmentPensionDetails = "معاش شهري 1800 ج.م", HasChronicDisease = true, ChronicDiseaseDetails = "سكري وضغط", NeedsPeriodicMedication = true, PeriodicMedicationDetails = "أدوية سكري وضغط", MedicationFrequency = "شهري", PriorityTier = "ب - متوسط الاحتياج", ResearchNotes = "يحتاج علاجًا دوريًا وتنسيقًا مع الجمعية الطبية." },
            new Beneficiary { Id = "demo-family-103", FamilyCode = "FAM-26-0103", FamilyName = "أسرة منى جابر", NationalId = "28802050100103", Name = "منى جابر محمد", Phone = "01000001003", RegisteredByCharityId = "c2", RegisteredByCharityName = "جمعية الأمل لرعاية الأيتام", Category = "أرامل وأيتام", MaritalStatus = "أرمل", FamilyCount = 3, Address = "القاهرة، المعادي", VillageOrDistrict = "المعادي", Housing = "إيجار جديد", MonthlyIncome = 2200, IncomeSource = "عمل جزئي", PriorityTier = "أ - أولوية قصوى ومعدم", ResearchNotes = "طفلان في التعليم ويحتاجان أدوات مدرسية وكفالة منتظمة." },
            new Beneficiary { Id = "demo-family-104", FamilyCode = "FAM-26-0104", FamilyName = "أسرة خالد رمضان", NationalId = "27911050100104", Name = "خالد رمضان يوسف", Phone = "01000001004", RegisteredByCharityId = "c2", RegisteredByCharityName = "جمعية الأمل لرعاية الأيتام", Category = "غارمين", MaritalStatus = "متزوج", FamilyCount = 5, Address = "القاهرة، دار السلام", VillageOrDistrict = "دار السلام", Housing = "إيجار جديد", MonthlyIncome = 3400, IncomeSource = "عمل حر", HasDebts = true, TotalDebtAmount = 18000, DebtCreditor = "تاجر أجهزة منزلية", IsThreatenedWithImprisonment = true, DebtNotes = "أقساط متأخرة وإيصالات أمانة؛ تحتاج مراجعة قانونية قبل أي صرف.", PriorityTier = "أ - أولوية قصوى ومعدم", ResearchNotes = "دعم قانوني ومراجعة حالة الدين، إلى جانب احتياج غذائي مؤقت." },
            new Beneficiary { Id = "demo-family-105", FamilyCode = "FAM-26-0105", FamilyName = "أسرة فاطمة السيد", NationalId = "28606220100105", Name = "فاطمة السيد محمود", Phone = "01000001005", RegisteredByCharityId = "c3", RegisteredByCharityName = "جمعية رسالة الصداقة", Category = "مرضى ومسنين", MaritalStatus = "مطلق", FamilyCount = 3, Address = "الإسكندرية، العصافرة", VillageOrDistrict = "العصافرة", Housing = "سكن مشترك", MonthlyIncome = 1600, IncomeSource = "مساعدة عائلية", HasChronicDisease = true, ChronicDiseaseDetails = "ضعف بصر", NeedsPeriodicMedication = true, PeriodicMedicationDetails = "قطرات عين", MedicationFrequency = "شهري", PriorityTier = "ب - متوسط الاحتياج", ResearchNotes = "تحتاج كشف رمد ونظارة مع متابعة صحية." },
            new Beneficiary { Id = "demo-family-106", FamilyCode = "FAM-26-0106", FamilyName = "أسرة إبراهيم لطفي", NationalId = "28101180100106", Name = "إبراهيم لطفي عبد الحميد", Phone = "01000001006", RegisteredByCharityId = "c3", RegisteredByCharityName = "جمعية رسالة الصداقة", Category = "حالات طوارئ", MaritalStatus = "متزوج", FamilyCount = 4, Address = "الإسكندرية، المنتزه", VillageOrDistrict = "المنتزه", Housing = "سكن متهالك/يحتاج ترميم", MonthlyIncome = 3100, IncomeSource = "يومية", PriorityTier = "ج - طوارئ وظرف مؤقت", RoofType = "جريد وقش وبوص", WallType = "طوب لبن/طيني", WaterSource = "طلمبة حبشية", SanitationType = "بدون صرف", ElectricitySource = "وصلة جيران", ResearchNotes = "تلف سقف المنزل بعد أمطار؛ أولوية لتقييم الترميم وتوفير بطاطين." }
        };
        foreach (var family in families)
            if (!await db.Beneficiaries.IgnoreQueryFilters().AnyAsync(x => x.Id == family.Id))
                await db.Beneficiaries.AddAsync(family);
        await db.SaveChangesAsync();

        var members = new[]
        {
            new BeneficiaryFamilyMember { Id = "demo-member-101a", BeneficiaryId = "demo-family-101", NationalId = "31301150101101", Name = "آية محمود أحمد", Relationship = "ابنة", MemberType = "معال", SupportType = "طالب", EducationStage = "إعدادي" },
            new BeneficiaryFamilyMember { Id = "demo-member-101b", BeneficiaryId = "demo-family-101", NationalId = "31605150101102", Name = "محمد محمود أحمد", Relationship = "ابن", MemberType = "معال", SupportType = "طالب", EducationStage = "ابتدائي" },
            new BeneficiaryFamilyMember { Id = "demo-member-103a", BeneficiaryId = "demo-family-103", NationalId = "31109120103101", Name = "سارة أحمد جابر", Relationship = "ابنة", MemberType = "معال", SupportType = "طالب", EducationStage = "ثانوي" },
            new BeneficiaryFamilyMember { Id = "demo-member-105a", BeneficiaryId = "demo-family-105", NationalId = "30508220105101", Name = "عمر حسن السيد", Relationship = "ابن", MemberType = "معيل", HasIncome = true, IncomeAmount = 1000, IncomeSource = "عمل جزئي", SupportType = "أخرى", EducationStage = "جامعي" }
        };
        foreach (var member in members)
            if (!await db.BeneficiaryFamilyMembers.IgnoreQueryFilters().AnyAsync(x => x.Id == member.Id))
                await db.BeneficiaryFamilyMembers.AddAsync(member);
        await db.SaveChangesAsync();

        var donations = new[]
        {
            new Donation { Id = "demo-donation-101", CharityId = "c1", DonorName = "فاعل خير - دعم غذائي", DonationKind = "نقدي", Amount = 12000, CashCategory = "صدقات عامة", PaymentMethod = "تحويل بنكي", Date = "2026-09-04", Notes = "تمويل برنامج المساعدات الغذائية لشهر سبتمبر" },
            new Donation { Id = "demo-donation-102", CharityId = "c1", DonorName = "شركة النور للتوريدات", DonationKind = "عيني", InventoryItemId = "i1", ItemName = "أرز مصري ممتاز", Quantity = 120, Unit = "كيلو", EstimatedValue = 4800, Date = "2026-09-12", Notes = "توريد عيني؛ الرصيد الافتتاحي بالمخزن يشمل هذه الدفعة" },
            new Donation { Id = "demo-donation-103", CharityId = "c1", DonorName = "فاعل خير - كفالة أسر", DonationKind = "نقدي", Amount = 7500, CashCategory = "زكاة مال", PaymentMethod = "نقداً بالخزينة", Date = "2026-10-02" },
            new Donation { Id = "demo-donation-201", CharityId = "c2", DonorName = "مبادرة دفء الشتاء", DonationKind = "عيني", InventoryItemId = "i_amal_1", ItemName = "بطاطين شتوية مبطنة", Quantity = 60, Unit = "قطعة", EstimatedValue = 9000, Date = "2026-09-18", Notes = "الرصيد الافتتاحي بالمخزن يشمل هذه الدفعة" },
            new Donation { Id = "demo-donation-202", CharityId = "c2", DonorName = "مجموعة أصدقاء الخير", DonationKind = "نقدي", Amount = 15000, CashCategory = "كفالات أيتام", PaymentMethod = "تحويل بنكي", Date = "2026-10-06" },
            new Donation { Id = "demo-donation-301", CharityId = "c3", DonorName = "عيادة الأمل للعيون", DonationKind = "خدمة", InventoryItemId = "s_sedaka_1", ItemName = "كشف رمد وعيون ونظارة طبية", Quantity = 12, Unit = "كشف ونظارة", EstimatedValue = 4200, Date = "2026-09-20" },
            new Donation { Id = "demo-donation-302", CharityId = "c3", DonorName = "صيدلية الشفاء", DonationKind = "عيني", InventoryItemId = "i_sedaka_2", ItemName = "أدوية سكر وضغط", Quantity = 30, Unit = "علبة", EstimatedValue = 2100, Date = "2026-10-03", Notes = "الرصيد الافتتاحي بالمخزن يشمل هذه الدفعة" }
        };
        foreach (var donation in donations)
            if (!await db.Donations.IgnoreQueryFilters().AnyAsync(x => x.Id == donation.Id))
                await db.Donations.AddAsync(donation);
        await db.SaveChangesAsync();

        var distributions = new[]
        {
            new Distribution { Id = "demo-aid-101", BeneficiaryId = "demo-family-101", CharityId = "c1", CharityName = "جمعية أبواب الخير", InventoryItemId = "i1", ItemName = "أرز مصري ممتاز", Kind = "صنف ملموس", Quantity = 8, Unit = "كيلو", Date = "2026-09-16", Notes = "دعم غذائي شهري موثق" },
            new Distribution { Id = "demo-aid-102", BeneficiaryId = "demo-family-101", CharityId = "c2", CharityName = "جمعية الأمل لرعاية الأيتام", InventoryItemId = "i_amal_2", ItemName = "شنطة أدوات مدرسية للأيتام", Kind = "صنف ملموس", Quantity = 2, Unit = "شنطة", Date = "2026-09-22", Notes = "دعم تعليمي مشترك لنفس الأسرة" },
            new Distribution { Id = "demo-aid-103", BeneficiaryId = "demo-family-102", CharityId = "c3", CharityName = "جمعية رسالة الصداقة", InventoryItemId = "i_sedaka_2", ItemName = "أدوية سكر وضغط", Kind = "صنف ملموس", Quantity = 2, Unit = "علبة", Date = "2026-10-05", Notes = "صرف علاج دوري بالتنسيق بين الجمعيات" },
            new Distribution { Id = "demo-aid-201", BeneficiaryId = "demo-family-103", CharityId = "c2", CharityName = "جمعية الأمل لرعاية الأيتام", InventoryItemId = "i_amal_2", ItemName = "شنطة أدوات مدرسية للأيتام", Kind = "صنف ملموس", Quantity = 2, Unit = "شنطة", Date = "2026-10-02" },
            new Distribution { Id = "demo-aid-301", BeneficiaryId = "demo-family-105", CharityId = "c3", CharityName = "جمعية رسالة الصداقة", InventoryItemId = "s_sedaka_1", ItemName = "كشف رمد وعيون ونظارة طبية", Kind = "خدمة", Quantity = 1, Unit = "كشف ونظارة", Date = "2026-10-10", Notes = "كشف وتقييم طبي مع توصية بنظارة" },
            new Distribution { Id = "demo-aid-302", BeneficiaryId = "demo-family-106", CharityId = "c2", CharityName = "جمعية الأمل لرعاية الأيتام", InventoryItemId = "i_amal_1", ItemName = "بطاطين شتوية مبطنة", Kind = "صنف ملموس", Quantity = 3, Unit = "قطعة", Date = "2026-10-12", Notes = "استجابة لحالة طوارئ سكنية" }
        };
        foreach (var distribution in distributions)
            if (!await db.Distributions.IgnoreQueryFilters().AnyAsync(x => x.Id == distribution.Id))
                await db.Distributions.AddAsync(distribution);
        await db.SaveChangesAsync();

        var staff = new[]
        {
            new StaffMember { Id = "demo-staff-researcher", CharityId = "c1", Name = "سلمى حسن", Phone = "01000002001", Type = "موظف براتب", Role = "باحثة اجتماعية", Salary = 5200, JoinDate = "2025-08-01", Address = "الجيزة، العمرانية", Tasks = "بحث الأسر وتحديث ملفات الاستحقاق" },
            new StaffMember { Id = "demo-staff-amal", CharityId = "c2", Name = "ندى إسماعيل", Phone = "01000002002", Type = "موظف براتب", Role = "منسقة كفالات", Salary = 4800, JoinDate = "2025-11-01", Address = "القاهرة، المعادي", Tasks = "تنسيق كفالات الأيتام وتسليم الأدوات المدرسية" },
            new StaffMember { Id = "demo-staff-sedaka", CharityId = "c3", Name = "ليلى مراد", Phone = "01000002003", Type = "متطوع", Role = "منسقة قوافل طبية", JoinDate = "2026-02-01", Address = "الإسكندرية، سموحة", Tasks = "تنسيق الكشف الطبي ومواعيد المتابعة" }
        };
        foreach (var member in staff)
            if (!await db.StaffMembers.IgnoreQueryFilters().AnyAsync(x => x.Id == member.Id))
                await db.StaffMembers.AddAsync(member);
        await db.SaveChangesAsync();
    }
}
