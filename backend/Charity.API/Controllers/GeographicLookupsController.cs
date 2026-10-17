using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Lookups;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/lookups")]
[Authorize]
public class GeographicLookupsController : ControllerBase
{
    private readonly IGeographicLookupService _geographicService;

    public GeographicLookupsController(IGeographicLookupService geographicService)
    {
        _geographicService = geographicService;
    }

    [HttpGet("governorates")]
    public async Task<ActionResult<IReadOnlyList<GovernorateDto>>> GetGovernorates()
    {
        var result = await _geographicService.GetGovernoratesAsync();
        return Ok(result);
    }

    [HttpGet("districts")]
    public async Task<ActionResult<IReadOnlyList<DistrictDto>>> GetDistricts([FromQuery] string? governorateId)
    {
        var result = await _geographicService.GetDistrictsAsync(governorateId);
        return Ok(result);
    }

    [HttpGet("addresses/autocomplete")]
    public async Task<ActionResult<IReadOnlyList<AddressAutocompleteItemDto>>> AutocompleteAddresses(
        [FromQuery] string? q,
        [FromQuery] string? governorateId,
        [FromQuery] string? districtId,
        [FromQuery] int limit = 10)
    {
        var result = await _geographicService.AutocompleteAddressesAsync(q, governorateId, districtId, limit);
        return Ok(result);
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<AddressRecordDto>> CreateAddress([FromBody] CreateAddressRecordDto dto)
    {
        var result = await _geographicService.CreateCustomAddressAsync(dto);
        return Ok(result);
    }
}
