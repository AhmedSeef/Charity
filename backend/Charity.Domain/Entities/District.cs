namespace Charity.Domain.Entities;

public class District : BaseEntity
{
    public string GovernorateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Governorate? Governorate { get; set; }
    public ICollection<AddressRecord> AddressRecords { get; set; } = new List<AddressRecord>();
    public ICollection<Village> Villages { get; set; } = new List<Village>();
}
