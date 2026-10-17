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
    public string EducationStage { get; set; } = "منتظم بالتعليم";
    public bool IsSchoolDropout { get; set; }
    public bool IsChildLabor { get; set; }

    // حالة الفصل من الملف
    public bool IsSeparated { get; set; }
    public DateTime? SeparatedAt { get; set; }
    public string? SeparatedToBeneficiaryId { get; set; }
    public string? SeparationReasonId { get; set; }
    public string? SeparationReason { get; set; }

    // هل هو عائل سابق للأسرة (سابق/متوفى)
    public bool IsFormerHead { get; set; }
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
    public string EducationStage { get; set; } = "منتظم بالتعليم";
    public bool IsSchoolDropout { get; set; }
    public bool IsChildLabor { get; set; }
}

public class BeneficiaryDto
{
    public string Id { get; set; } = string.Empty;
    public string FamilyCode { get; set; } = string.Empty; // كود العائلة الدائم مثل FAM-26-0001
    public string FamilyName { get; set; } = string.Empty; // مسمى العائلة الموحد
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

    // 4. أبعاد البحث الاجتماعي الميداني والقرية
    public string VillageOrDistrict { get; set; } = string.Empty;
    public string PriorityTier { get; set; } = "أ - أولوية قصوى ومعدم";
    public string RoofType { get; set; } = "خرساني مسلح";
    public string WallType { get; set; } = "طوب أحمر ومحارة";
    public string WaterSource { get; set; } = "عداد مياه رسمي";
    public string SanitationType { get; set; } = "شبكة صرف صحي عمومية";
    public string ElectricitySource { get; set; } = "عداد قانوني";
    public string AppliancesSummary { get; set; } = string.Empty;
    public bool HasDebts { get; set; }
    public decimal? TotalDebtAmount { get; set; }
    public string DebtCreditor { get; set; } = string.Empty;
    public bool IsThreatenedWithImprisonment { get; set; }
    public string DebtNotes { get; set; } = string.Empty;
    public bool IsCandidateForMicroProject { get; set; }
    public string SuggestedMicroProject { get; set; } = string.Empty;

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

    // سجل تعاقب ولاية الأسرة (التاريخ الكامل للعائلين السابقين)
    public List<BeneficiaryHeadHistoryDto> HeadHistories { get; set; } = new();
}

public class CreateBeneficiaryDto
{
    public string? FamilyCode { get; set; } // اختياري - يتم توليده تلقائياً إن ترك فارغاً
    public string? FamilyName { get; set; } // مسمى العائلة مثل عائلة فلان الفلاني
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

    // 4. أبعاد البحث الاجتماعي الميداني والقرية
    public string VillageOrDistrict { get; set; } = string.Empty;
    public string PriorityTier { get; set; } = "أ - أولوية قصوى ومعدم";
    public string RoofType { get; set; } = "خرساني مسلح";
    public string WallType { get; set; } = "طوب أحمر ومحارة";
    public string WaterSource { get; set; } = "عداد مياه رسمي";
    public string SanitationType { get; set; } = "شبكة صرف صحي عمومية";
    public string ElectricitySource { get; set; } = "عداد قانوني";
    public string AppliancesSummary { get; set; } = string.Empty;
    public bool HasDebts { get; set; } = false;
    public decimal? TotalDebtAmount { get; set; }
    public string DebtCreditor { get; set; } = string.Empty;
    public bool IsThreatenedWithImprisonment { get; set; } = false;
    public string DebtNotes { get; set; } = string.Empty;
    public bool IsCandidateForMicroProject { get; set; } = false;
    public string SuggestedMicroProject { get; set; } = string.Empty;

    // أفراد الأسرة
    public List<CreateFamilyMemberDto> FamilyMembers { get; set; } = new();
}

public class UpdateBeneficiaryDto
{
    public string? FamilyCode { get; set; }
    public string? FamilyName { get; set; }
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

    // 4. أبعاد البحث الاجتماعي الميداني والقرية
    public string VillageOrDistrict { get; set; } = string.Empty;
    public string PriorityTier { get; set; } = "أ - أولوية قصوى ومعدم";
    public string RoofType { get; set; } = "خرساني مسلح";
    public string WallType { get; set; } = "طوب أحمر ومحارة";
    public string WaterSource { get; set; } = "عداد مياه رسمي";
    public string SanitationType { get; set; } = "شبكة صرف صحي عمومية";
    public string ElectricitySource { get; set; } = "عداد قانوني";
    public string AppliancesSummary { get; set; } = string.Empty;
    public bool HasDebts { get; set; } = false;
    public decimal? TotalDebtAmount { get; set; }
    public string DebtCreditor { get; set; } = string.Empty;
    public bool IsThreatenedWithImprisonment { get; set; } = false;
    public string DebtNotes { get; set; } = string.Empty;
    public bool IsCandidateForMicroProject { get; set; } = false;
    public string SuggestedMicroProject { get; set; } = string.Empty;

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
    public string? SeparationReasonId { get; set; }
    public string? Reason { get; set; }
}

public class BeneficiaryHeadHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string BeneficiaryId { get; set; } = string.Empty;
    public string PreviousHeadName { get; set; } = string.Empty;
    public string PreviousHeadNationalId { get; set; } = string.Empty;
    public string NewHeadName { get; set; } = string.Empty;
    public string NewHeadNationalId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; }
    public string TransferredByCharityName { get; set; } = string.Empty;
}

public class TransferHeadshipDto
{
    public string NewHeadFamilyMemberId { get; set; } = string.Empty; // الفرد المراد ترقيته كعائل جديد
    public string FormerHeadAction { get; set; } = "KeepAsFamilyMember"; // "KeepAsFamilyMember" (نقل مع بقاء) أو "RemoveFromFamily" (نقل مع حذف/استبعاد)
    public string Reason { get; set; } = "عجز كلي أو مرض مقعد"; // وفاة، عجز كامل أو مرض، انفصال...
    public string? FormerHeadRelationship { get; set; } = "زوج (عائل سابق)";
    public string? FormerHeadSupportType { get; set; } = "مريض / عاجز عن العمل";
    public string? NewFamilyName { get; set; } // مسمى العائلة المحدث اختياري
    public string? Notes { get; set; }
}
