namespace Charity.Application.DTOs.Beneficiaries;

public class FamilyMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string BeneficiaryId { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = "ابن/ابنة";
    public string MemberType { get; set; } = "معال"; // "معال" (يعوله) أو "معيل" (يعول صاحب الملف)
    public bool HasIncome { get; set; }
    public decimal? IncomeAmount { get; set; }
    public string IncomeSource { get; set; } = string.Empty;
    public string SupportType { get; set; } = "طالب"; // طالب، مريض، طفل، مسن، عاطل، ذوي همم، أخرى
    public string HealthStatus { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // حالة الفصل من الملف
    public bool IsSeparated { get; set; }
    public DateTime? SeparatedAt { get; set; }
    public string? SeparatedToBeneficiaryId { get; set; }
    public string? SeparationReason { get; set; }
}

public class CreateFamilyMemberDto
{
    public string? Id { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = "ابن/ابنة";
    public string MemberType { get; set; } = "معال";
    public bool HasIncome { get; set; }
    public decimal? IncomeAmount { get; set; }
    public string IncomeSource { get; set; } = string.Empty;
    public string SupportType { get; set; } = "طالب";
    public string HealthStatus { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class BeneficiaryDto
{
    public string Id { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public int FamilyCount { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Housing { get; set; } = string.Empty;
    public string HealthIssues { get; set; } = string.Empty;
    public string ResearchNotes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // الدخل والممتلكات والمعاش الحكومي
    public string IncomeSource { get; set; } = string.Empty;
    public decimal? MonthlyIncome { get; set; }
    public bool HasLandOrProperty { get; set; }
    public string LandOrPropertyDetails { get; set; } = string.Empty;
    public bool HasGovernmentPension { get; set; }
    public string GovernmentPensionDetails { get; set; } = string.Empty;

    // الأمراض المزمنة والعلاج الدوري
    public bool HasChronicDisease { get; set; }
    public string ChronicDiseaseDetails { get; set; } = string.Empty;
    public bool NeedsPeriodicMedication { get; set; }
    public string PeriodicMedicationDetails { get; set; } = string.Empty;
    public string MedicationFrequency { get; set; } = string.Empty;

    // حالة الإيقاف ومنع الصرف
    public bool IsSuspended { get; set; }
    public string? SuspensionReasonId { get; set; }
    public string? SuspensionReasonTitle { get; set; }
    public string? SuspensionNotes { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendedByCharityId { get; set; }
    public string? SuspendedByCharityName { get; set; }

    // الحوكمة والشراكة
    public string RegisteredByCharityId { get; set; } = string.Empty;
    public string RegisteredByCharityName { get; set; } = string.Empty;
    public bool IsEditableByCurrentCharity { get; set; }

    // أفراد الأسرة والمعالين والمعيلين
    public List<FamilyMemberDto> FamilyMembers { get; set; } = new();
}

public class CreateBeneficiaryDto
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

    // الدخل والممتلكات والمعاش الحكومي
    public string IncomeSource { get; set; } = string.Empty;
    public decimal? MonthlyIncome { get; set; }
    public bool HasLandOrProperty { get; set; }
    public string LandOrPropertyDetails { get; set; } = string.Empty;
    public bool HasGovernmentPension { get; set; }
    public string GovernmentPensionDetails { get; set; } = string.Empty;

    // الأمراض المزمنة والعلاج الدوري
    public bool HasChronicDisease { get; set; }
    public string ChronicDiseaseDetails { get; set; } = string.Empty;
    public bool NeedsPeriodicMedication { get; set; }
    public string PeriodicMedicationDetails { get; set; } = string.Empty;
    public string MedicationFrequency { get; set; } = string.Empty;

    // أفراد الأسرة
    public List<CreateFamilyMemberDto> FamilyMembers { get; set; } = new();
}

public class UpdateBeneficiaryDto
{
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string MaritalStatus { get; set; } = string.Empty;
    public int FamilyCount { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Housing { get; set; } = string.Empty;
    public string HealthIssues { get; set; } = string.Empty;
    public string ResearchNotes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // الدخل والممتلكات والمعاش الحكومي
    public string IncomeSource { get; set; } = string.Empty;
    public decimal? MonthlyIncome { get; set; }
    public bool HasLandOrProperty { get; set; }
    public string LandOrPropertyDetails { get; set; } = string.Empty;
    public bool HasGovernmentPension { get; set; }
    public string GovernmentPensionDetails { get; set; } = string.Empty;

    // الأمراض المزمنة والعلاج الدوري
    public bool HasChronicDisease { get; set; }
    public string ChronicDiseaseDetails { get; set; } = string.Empty;
    public bool NeedsPeriodicMedication { get; set; }
    public string PeriodicMedicationDetails { get; set; } = string.Empty;
    public string MedicationFrequency { get; set; } = string.Empty;

    // أفراد الأسرة
    public List<CreateFamilyMemberDto> FamilyMembers { get; set; } = new();
}

public class SuspendBeneficiaryDto
{
    public string? SuspensionReasonId { get; set; }
    public string SuspensionReasonTitle { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class SeparateFamilyMemberDto
{
    public string FamilyMemberId { get; set; } = string.Empty;
    public bool CreateIndependentBeneficiaryProfile { get; set; } = true;
    public string? Reason { get; set; }
}
