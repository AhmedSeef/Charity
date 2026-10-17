using Charity.Application.DTOs.Lookups;

namespace Charity.Application.Services.Interfaces;

public interface IGeographicLookupService
{
    Task<IReadOnlyList<GovernorateDto>> GetGovernoratesAsync();
    Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(string? governorateId = null);
    Task<IReadOnlyList<AddressAutocompleteItemDto>> AutocompleteAddressesAsync(string? query, string? governorateId = null, string? districtId = null, int limit = 10);
    Task<AddressRecordDto> RecordAddressUsageAsync(CreateAddressRecordDto dto);
    Task<AddressRecordDto> CreateCustomAddressAsync(CreateAddressRecordDto dto);
}
