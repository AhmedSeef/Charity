using System.Security.Claims;
using Charity.Application.Services.Interfaces;

namespace Charity.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Username => User?.Identity?.Name;
    public string? FullName => User?.FindFirstValue("FullName");

    public string? CharityId => User?.FindFirstValue("CharityId");

    public string? CharityName => User?.FindFirstValue("CharityName");

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Permissions => 
        User?.FindAll("permission").Select(c => c.Value).Distinct().ToList() ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool HasPermission(string permission)
    {
        if (Role == "Admin" || Role == "CharityManager")
            return true;

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
