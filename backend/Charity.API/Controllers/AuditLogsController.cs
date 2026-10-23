using Charity.Application.DTOs.Audit;
using Charity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charity.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "CharityManager")]
public sealed class AuditLogsController(IAuditLogService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuditLogPageDto>> Get([FromQuery] string? name, [FromQuery] string? action,
        [FromQuery] string? entityType, [FromQuery] string? from, [FromQuery] string? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) =>
        Ok(await service.GetAsync(name, action, entityType, from, to, page, pageSize, cancellationToken));
}
