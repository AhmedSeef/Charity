namespace Charity.Domain.Entities;

public class SeparationReason : BaseEntity
{
    public string Title { get; set; } = string.Empty; // عنوان سبب الانفصال (مثال: زواج، استقلال بمسكن، تكوين أسرة...)
    public string Description { get; set; } = string.Empty; // شرح وتفاصيل السبب
    public bool IsActive { get; set; } = true;
    public string? CharityId { get; set; } // الجمعية التي أضافت السبب أو null للنظام العام
    public string? CharityName { get; set; }
}
