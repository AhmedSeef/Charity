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

    // Data Ownership & Governance: ONLY this charity can edit!
    public string RegisteredByCharityId { get; set; } = string.Empty;
    public string RegisteredByCharityName { get; set; } = string.Empty;
    public Charity? RegisteredByCharity { get; set; }

    // Navigation to cross-charity distribution records (aid history)
    public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();

    // Navigation to family members (معالين ومعيلين)
    public ICollection<BeneficiaryFamilyMember> FamilyMembers { get; set; } = new List<BeneficiaryFamilyMember>();
}
