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

    // ==========================================
    // Villages (القرى)
    // ==========================================

    [HttpGet("villages")]
    public async Task<ActionResult<IReadOnlyList<VillageDto>>> GetVillages([FromQuery] string? districtId)
    {
        var result = await _geographicService.GetVillagesAsync(districtId);
        return Ok(result);
    }

    [HttpGet("villages/{id}")]
    public async Task<ActionResult<VillageDto>> GetVillageById(string id)
    {
        var result = await _geographicService.GetVillageByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("villages")]
    public async Task<ActionResult<VillageDto>> CreateVillage([FromBody] CreateVillageDto dto)
    {
        var result = await _geographicService.CreateVillageAsync(dto);
        return CreatedAtAction(nameof(GetVillageById), new { id = result.Id }, result);
    }

    [HttpPut("villages/{id}")]
    public async Task<ActionResult<VillageDto>> UpdateVillage(string id, [FromBody] UpdateVillageDto dto)
    {
        var result = await _geographicService.UpdateVillageAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("villages/{id}")]
    public async Task<ActionResult> DeleteVillage(string id)
    {
        await _geographicService.DeleteVillageAsync(id);
        return NoContent();
    }

    // ==========================================
    // Village Areas (المناطق والأحياء التابعة للقرى)
    // ==========================================

    [HttpGet("village-areas")]
    public async Task<ActionResult<IReadOnlyList<VillageAreaDto>>> GetVillageAreas([FromQuery] string? villageId)
    {
        var result = await _geographicService.GetVillageAreasAsync(villageId);
        return Ok(result);
    }

    [HttpGet("village-areas/{id}")]
    public async Task<ActionResult<VillageAreaDto>> GetVillageAreaById(string id)
    {
        var result = await _geographicService.GetVillageAreaByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("village-areas")]
    public async Task<ActionResult<VillageAreaDto>> CreateVillageArea([FromBody] CreateVillageAreaDto dto)
    {
        var result = await _geographicService.CreateVillageAreaAsync(dto);
        return CreatedAtAction(nameof(GetVillageAreaById), new { id = result.Id }, result);
    }

    [HttpPut("village-areas/{id}")]
    public async Task<ActionResult<VillageAreaDto>> UpdateVillageArea(string id, [FromBody] UpdateVillageAreaDto dto)
    {
        var result = await _geographicService.UpdateVillageAreaAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("village-areas/{id}")]
    public async Task<ActionResult> DeleteVillageArea(string id)
    {
        await _geographicService.DeleteVillageAreaAsync(id);
        return NoContent();
    }

    // ==========================================
    // Addresses & Autocomplete
    // ==========================================

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
