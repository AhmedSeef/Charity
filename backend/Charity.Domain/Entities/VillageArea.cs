namespace Charity.Domain.Entities;

public class VillageArea : BaseEntity
{
    public string VillageId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Navigations
    public Village? Village { get; set; }
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
}
