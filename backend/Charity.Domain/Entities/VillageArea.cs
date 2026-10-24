namespace Charity.Domain.Entities;

public class VillageArea : BaseEntity
{
    // Exactly one parent is required: a village or a district/quarter directly.
    public string? VillageId { get; set; }
    public string? DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Navigations
    public Village? Village { get; set; }
    public District? District { get; set; }
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
}
