namespace Charity.Domain.Entities;

// Immutable operation record. It intentionally does not derive from BaseEntity.
public sealed class AuditLog
{
    public long Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string CharityId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
}
