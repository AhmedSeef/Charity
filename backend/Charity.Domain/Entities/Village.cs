namespace Charity.Domain.Entities;

public class Village : BaseEntity
{
    public string DistrictId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Navigations
    public District? District { get; set; }
    public ICollection<VillageArea> Areas { get; set; } = new List<VillageArea>();
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
}
