namespace Charity.Domain.Entities;

public class BeneficiaryFamilyMember : BaseEntity
{
    public string BeneficiaryId { get; set; } = string.Empty;
    public Beneficiary? Beneficiary { get; set; }

    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = "ابن/ابنة"; // صلة القرابة
    public string MemberType { get; set; } = "معال"; // "معال" (يعوله المستفيد) أو "معيل" (يعول صاحب الملف)
    
    // بيانات الدخل للفرد
    public bool HasIncome { get; set; }
    public decimal? IncomeAmount { get; set; }
    public string IncomeSource { get; set; } = string.Empty;

    // نوع الدعم والاحتياج
    public string SupportType { get; set; } = "طالب"; // طالب، مريض، طفل، مسن، عاطل، ذوي همم، أخرى
    public string HealthStatus { get; set; } = string.Empty; // الحالة الصحية للفرد إن وجدت
    public string Notes { get; set; } = string.Empty;

    // التعليم والتسرب الدراسي وعمالة الأطفال
    public string EducationStage { get; set; } = "منتظم بالتعليم"; // ابتدائي، إعدادي، ثانوي، جامعي، متسرب من التعليم، لم يلتحق / أمي
    public bool IsSchoolDropout { get; set; } = false; // هل متسرب من التعليم؟
    public bool IsChildLabor { get; set; } = false; // هل طفل عامل دون السن القانوني؟

    // حالة الفصل من الملف
    public bool IsSeparated { get; set; } = false;
    public DateTime? SeparatedAt { get; set; }
    public string? SeparatedToBeneficiaryId { get; set; }
    public string? SeparationReason { get; set; }

    // هل كان عائلاً سابقاً للأسرة (رب أسرة سابق متوفى أو تنازل عن الولاية)
    public bool IsFormerHead { get; set; } = false;
}
