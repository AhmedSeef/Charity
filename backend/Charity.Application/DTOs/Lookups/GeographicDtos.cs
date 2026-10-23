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
    public int VillagesCount { get; set; }
}

public class VillageDto
{
    public string Id { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string GovernorateId { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int AreasCount { get; set; }
    public int BeneficiariesCount { get; set; }
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }
}

public class CreateVillageDto
{
    public string DistrictId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class UpdateVillageDto
{
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class VillageAreaDto
{
    public string Id { get; set; } = string.Empty;
    public string VillageId { get; set; } = string.Empty;
    public string VillageName { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string GovernorateId { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int BeneficiariesCount { get; set; }
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }
}

public class CreateVillageAreaDto
{
    public string VillageId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class UpdateVillageAreaDto
{
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
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
