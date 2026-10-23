namespace Charity.Application.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? FullName { get; }
    string? CharityId { get; }
    string? CharityName { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permission);
}
