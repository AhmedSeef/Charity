namespace Charity.Domain.Entities;

public class SuspensionReason : BaseEntity
{
    public string Title { get; set; } = string.Empty; // عنوان سبب الإيقاف (مثال: وفاة، تحسن الدخل، زواج...)
    public string Description { get; set; } = string.Empty; // شرح وتفاصيل السبب
    public bool IsActive { get; set; } = true;
    public string CharityId { get; set; } = string.Empty; // الجمعية التي أضافت السبب أو عام
    public string CharityName { get; set; } = string.Empty;
}
