using Charity.Application.DTOs.Audit;

namespace Charity.Application.Services.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogPageDto> GetAsync(string? name, string? action, string? entityType,
        string? from, string? to, int page, int pageSize, CancellationToken cancellationToken = default);
}
