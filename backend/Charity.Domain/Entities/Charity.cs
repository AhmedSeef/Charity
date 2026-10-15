namespace Charity.Domain.Entities;

public class Charity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RoleDesc { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Color { get; set; } = "#004d40";
    public string Icon { get; set; } = "🏢";
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;

    // Navigations
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Beneficiary> RegisteredBeneficiaries { get; set; } = new List<Beneficiary>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();
    public ICollection<StaffMember> StaffMembers { get; set; } = new List<StaffMember>();
}
