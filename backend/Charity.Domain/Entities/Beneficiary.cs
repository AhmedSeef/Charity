namespace Charity.Domain.Entities;

public class Beneficiary : BaseEntity
{
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = "أرامل وأيتام";
    public string MaritalStatus { get; set; } = "متزوج";
    public int FamilyCount { get; set; } = 1;
    public string Address { get; set; } = string.Empty;
    public string Housing { get; set; } = "إيجار جديد";
    public string HealthIssues { get; set; } = string.Empty;
    public string ResearchNotes { get; set; } = string.Empty;
    public string Status { get; set; } = "مستحق فعال";

    // 1. الدخل والممتلكات والمعاش الحكومي
    public string IncomeSource { get; set; } = string.Empty; // مصدر الدخل (يومية، عمل حر، لا يعمل...)
    public decimal? MonthlyIncome { get; set; } // الدخل الشهري
    public bool HasLandOrProperty { get; set; } // هل يملك أراضي أو عقارات؟
    public string LandOrPropertyDetails { get; set; } = string.Empty; // تفاصيل الأراضي أو الأملاك
    public bool HasGovernmentPension { get; set; } // هل يحصل على معاش شهري حكومي؟
    public string GovernmentPensionDetails { get; set; } = string.Empty; // تفاصيل المعاش (تكافل وكرامة، تأمينات...) وقيمته

    // 2. الأمراض المزمنة والعلاج الدوري
    public bool HasChronicDisease { get; set; } // هل يعاني من مرض مزمن؟
    public string ChronicDiseaseDetails { get; set; } = string.Empty; // طبيعة المرض المزمن
    public bool NeedsPeriodicMedication { get; set; } // هل يحتاج علاج دوري؟
    public string PeriodicMedicationDetails { get; set; } = string.Empty; // طبيعة ونوع العلاج الدوري
    public string MedicationFrequency { get; set; } = string.Empty; // كل قد إيه (أسبوعي، شهري، ربع سنوي...)

    // 3. أسباب الإيقاف ومنع الصرف (Suspension & Block Aid)
    public bool IsSuspended { get; set; } = false;
    public string? SuspensionReasonId { get; set; }
    public string? SuspensionReasonTitle { get; set; }
    public string? SuspensionNotes { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendedByCharityId { get; set; }
    public string? SuspendedByCharityName { get; set; }

    // 4. أبعاد البحث الاجتماعي الميداني والقرية (Field Research & Multidimensional Poverty)
    public string VillageOrDistrict { get; set; } = string.Empty; // القرية / العزبة / النجع / الحي
    public string PriorityTier { get; set; } = "أ - أولوية قصوى ومعدم"; // أ - أولوية قصوى ومعدم | ب - متوسط الاحتياج | ج - طوارئ وظرف مؤقت

    // مواصفات السكن والبيئة التحتية (تسقيف البيوت والمرافق)
    public string RoofType { get; set; } = "خرساني مسلح"; // خرساني مسلح، خشب وألواح، جريد وقش وبوص، صاج وزينك، بدون سقف/مكشوف
    public string WallType { get; set; } = "طوب أحمر ومحارة"; // طوب أحمر، طوب لبن/طيني، صاج وخشب
    public string WaterSource { get; set; } = "عداد مياه رسمي"; // عداد مياه رسمي، طلمبة حبشية، ملء من الجيران، بدون مياه
    public string SanitationType { get; set; } = "شبكة صرف صحي عمومية"; // شبكة صرف عمومية، بيارة/طرنش، بدون صرف
    public string ElectricitySource { get; set; } = "عداد قانوني"; // عداد قانوني، عداد كودي/ممارسة، وصلة جيران، بدون كهرباء
    public string AppliancesSummary { get; set; } = string.Empty; // نواقص الأجهزة أو المتوفر (ثلاجة، غسالة، بوتاجاز...)

    // ملف الديون وإيصالات الأمانة (ملف الغارمين)
    public bool HasDebts { get; set; } = false; // هل على الأسرة ديون أو كمبيالات؟
    public decimal? TotalDebtAmount { get; set; } // إجمالي قيمة الدين
    public string DebtCreditor { get; set; } = string.Empty; // تاجر أجهزة وتجهيز عرائس، شركة تمويل متناهي صغر، أفراد
    public bool IsThreatenedWithImprisonment { get; set; } = false; // هل مهدد بالحبس أو عليه قضايا؟
    public string DebtNotes { get; set; } = string.Empty; // تفاصيل الديون والأحكام

    // التمكين الاقتصادي والمشروعات متناهية الصغر
    public bool IsCandidateForMicroProject { get; set; } = false; // هل مرشح لمشروع تمكين؟
    public string SuggestedMicroProject { get; set; } = string.Empty; // ماكينة خياطة، تروسيكل، تربية مواشي/دواجن، كشك...

    // Data Ownership & Governance: ONLY this charity can edit!
    public string RegisteredByCharityId { get; set; } = string.Empty;
    public string RegisteredByCharityName { get; set; } = string.Empty;
    public Charity? RegisteredByCharity { get; set; }

    // Navigation to cross-charity distribution records (aid history)
    public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();

    // Navigation to family members (معالين ومعيلين)
    public ICollection<BeneficiaryFamilyMember> FamilyMembers { get; set; } = new List<BeneficiaryFamilyMember>();
}
