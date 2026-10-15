namespace Charity.Domain.Entities;

/// <summary>
/// توثيق تاريخ تعاقب ولاية الأسرة وعائل الملف (Succession Audit Trail)
/// يحفظ العائل السابق وتاريخ النقل وسبب النقل (وفاة، عجز...) لضمان بقاء السجل التاريخي للأبد
/// </summary>
public class BeneficiaryHeadHistory : BaseEntity
{
    public string BeneficiaryId { get; set; } = string.Empty;
    public Beneficiary? Beneficiary { get; set; }

    public string PreviousHeadName { get; set; } = string.Empty;
    public string PreviousHeadNationalId { get; set; } = string.Empty;

    public string NewHeadName { get; set; } = string.Empty;
    public string NewHeadNationalId { get; set; } = string.Empty;

    public string Reason { get; set; } = "وفاة رب الأسرة السابق"; // وفاة، عجز كامل أو مرض، انفصال...
    public string Notes { get; set; } = string.Empty;
    public DateTime TransferredAt { get; set; } = DateTime.UtcNow;

    public string TransferredByCharityId { get; set; } = string.Empty;
    public string TransferredByCharityName { get; set; } = string.Empty;
}
