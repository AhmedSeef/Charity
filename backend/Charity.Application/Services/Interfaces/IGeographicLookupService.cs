using Charity.Application.DTOs.Lookups;

namespace Charity.Application.Services.Interfaces;

public interface IGeographicLookupService
{
    // Governorates & Districts
    Task<IReadOnlyList<GovernorateDto>> GetGovernoratesAsync();
    Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(string? governorateId = null);

    // Villages (القرى)
    Task<IReadOnlyList<VillageDto>> GetVillagesAsync(string? districtId = null);
    Task<VillageDto> GetVillageByIdAsync(string id);
    Task<VillageDto> CreateVillageAsync(CreateVillageDto dto);
    Task<VillageDto> UpdateVillageAsync(string id, UpdateVillageDto dto);
    Task DeleteVillageAsync(string id);

    // Village Areas (المناطق والأحياء التابعة للقرى)
    Task<IReadOnlyList<VillageAreaDto>> GetVillageAreasAsync(string? villageId = null);
    Task<VillageAreaDto> GetVillageAreaByIdAsync(string id);
    Task<VillageAreaDto> CreateVillageAreaAsync(CreateVillageAreaDto dto);
    Task<VillageAreaDto> UpdateVillageAreaAsync(string id, UpdateVillageAreaDto dto);
    Task DeleteVillageAreaAsync(string id);

    // Addresses & Autocomplete
    Task<IReadOnlyList<AddressAutocompleteItemDto>> AutocompleteAddressesAsync(string? query, string? governorateId = null, string? districtId = null, int limit = 10);
    Task<AddressRecordDto> RecordAddressUsageAsync(CreateAddressRecordDto dto);
    Task<AddressRecordDto> CreateCustomAddressAsync(CreateAddressRecordDto dto);
}
