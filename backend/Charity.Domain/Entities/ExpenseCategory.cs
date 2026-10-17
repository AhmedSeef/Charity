namespace Charity.Domain.Entities;

public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Tenant / System-wide
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Navigation
    public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();
}
