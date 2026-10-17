namespace Charity.Application.DTOs.Lookups;

public class GovernorateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int DistrictsCount { get; set; }
}

public class DistrictDto
{
    public string Id { get; set; } = string.Empty;
    public string GovernorateId { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class AddressRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string GovernorateId { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}

public class AddressAutocompleteItemDto
{
    public string Id { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string GovernorateId { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public int UsageCount { get; set; }
}

public class CreateAddressRecordDto
{
    public string GovernorateId { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
}
