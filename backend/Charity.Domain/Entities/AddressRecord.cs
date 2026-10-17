namespace Charity.Domain.Entities;

public class AddressRecord : BaseEntity
{
    public string GovernorateId { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public string NormalizedText { get; set; } = string.Empty;
    public int UsageCount { get; set; } = 1;
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Navigations
    public Governorate? Governorate { get; set; }
    public District? District { get; set; }
}
