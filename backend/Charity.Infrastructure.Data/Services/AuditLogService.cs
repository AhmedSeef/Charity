using System.Globalization;
using Charity.Application.DTOs.Audit;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Exceptions;
using Charity.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Charity.Infrastructure.Data.Services;

public sealed class AuditLogService(CharityDbContext db, ICurrentUserService currentUser) : IAuditLogService
{
    public async Task<AuditLogPageDto> GetAsync(string? name, string? action, string? entityType,
        string? from, string? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (currentUser.Role != "CharityManager" || string.IsNullOrWhiteSpace(currentUser.CharityId))
            throw new UnauthorizedDomainException("سجل العمليات متاح لمدير الجمعية فقط.");

        if (page < 1 || pageSize is < 1 or > 100)
            throw new ValidationDomainException("رقم الصفحة أو حجمها غير صحيح.");

        var query = db.AuditLogs.AsNoTracking().Where(x => x.CharityId == currentUser.CharityId);
        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = $"%{EscapeLike(name.Trim())}%";
            query = query.Where(x => EF.Functions.Like(x.Username, pattern, "\\") ||
                EF.Functions.Like(x.ActorName, pattern, "\\"));
        }
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action.Trim());
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType.Trim());

        var fromDate = string.IsNullOrWhiteSpace(from) ? (DateOnly?)null : ParseDate(from, "تاريخ البداية");
        var toDate = string.IsNullOrWhiteSpace(to) ? (DateOnly?)null : ParseDate(to, "تاريخ النهاية");
        if (fromDate > toDate)
            throw new ValidationDomainException("تاريخ البداية يجب ألا يكون بعد تاريخ النهاية.");
        var cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        if (fromDate.HasValue)
        {
            var utc = TimeZoneInfo.ConvertTimeToUtc(fromDate.Value.ToDateTime(TimeOnly.MinValue), cairo);
            query = query.Where(x => x.OccurredAt >= utc);
        }
        if (toDate.HasValue)
        {
            var utc = TimeZoneInfo.ConvertTimeToUtc(toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), cairo);
            query = query.Where(x => x.OccurredAt < utc);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id, OccurredAt = x.OccurredAt, UserId = x.UserId,
                Username = x.Username, ActorName = x.ActorName, Action = x.Action,
                EntityType = x.EntityType, EntityId = x.EntityId, EntityName = x.EntityName
            }).ToListAsync(cancellationToken);
        return new AuditLogPageDto { Items = items, TotalCount = count, Page = page, PageSize = pageSize };
    }

    private static DateOnly ParseDate(string value, string field) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed : throw new ValidationDomainException($"{field} غير صحيح. استخدم yyyy-MM-dd.");

    private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
}
