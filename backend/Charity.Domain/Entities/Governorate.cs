namespace Charity.Domain.Entities;

public class Governorate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<District> Districts { get; set; } = new List<District>();
}
