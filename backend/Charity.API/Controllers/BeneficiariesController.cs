using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Distributions;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BeneficiariesController : ControllerBase
{
    private readonly IBeneficiaryService _beneficiaryService;

    public BeneficiariesController(IBeneficiaryService beneficiaryService)
    {
        _beneficiaryService = beneficiaryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BeneficiaryDto>>> GetAll()
    {
        var result = await _beneficiaryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BeneficiaryDto>> GetById(string id)
    {
        var result = await _beneficiaryService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "المستفيد غير موجود" });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<BeneficiaryDto>> Create([FromBody] CreateBeneficiaryDto dto)
    {
        var result = await _beneficiaryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BeneficiaryDto>> Update(string id, [FromBody] UpdateBeneficiaryDto dto)
    {
        var result = await _beneficiaryService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpGet("{id}/aid-history")]
    public async Task<ActionResult<BeneficiaryAidSummaryDto>> GetAidHistory(string id)
    {
        var result = await _beneficiaryService.GetAidSummaryAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/suspend")]
    public async Task<ActionResult<BeneficiaryDto>> Suspend(string id, [FromBody] SuspendBeneficiaryDto dto)
    {
        var result = await _beneficiaryService.SuspendAsync(id, dto);
        return Ok(result);
    }

    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<BeneficiaryDto>> Reactivate(string id)
    {
        var result = await _beneficiaryService.ReactivateAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/members/separate")]
    public async Task<ActionResult<BeneficiaryDto>> SeparateFamilyMember(string id, [FromBody] SeparateFamilyMemberDto dto)
    {
        var result = await _beneficiaryService.SeparateFamilyMemberAsync(id, dto);
        return Ok(result);
    }
}
