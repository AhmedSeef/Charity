using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Charity.Application.DTOs.Auth;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<Charity.Domain.Entities.Charity> _charityRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IRepository<Charity.Domain.Entities.Charity> charityRepo,
        ICurrentUserService currentUserService,
        IConfiguration config)
    {
        _userManager = userManager;
        _charityRepo = charityRepo;
        _currentUserService = currentUserService;
        _config = config;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            throw new UnauthorizedDomainException("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedDomainException("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "CharityManager";
        var charity = !string.IsNullOrEmpty(user.CharityId) ? await _charityRepo.GetByIdAsync(user.CharityId) : null;
        var charityName = charity?.Name ?? (role == "Admin" ? "إدارة شبكة الجمعيات" : string.Empty);

        var userClaims = await _userManager.GetClaimsAsync(user);
        var permissions = (role == "Admin" || role == "CharityManager")
            ? Charity.Domain.Security.AppPermissions.All.ToList()
            : userClaims.Where(c => c.Type == "permission").Select(c => c.Value).Distinct().ToList();

        var token = GenerateJwtToken(user, charityName, role, permissions);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            FullName = user.FullName,
            CharityId = user.CharityId ?? string.Empty,
            CharityName = charityName,
            Role = role,
            Permissions = permissions
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return null;

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? _currentUserService.Role ?? "User";
        var charity = !string.IsNullOrEmpty(user.CharityId) ? await _charityRepo.GetByIdAsync(user.CharityId) : null;
        var charityName = charity?.Name ?? (role == "Admin" ? "إدارة شبكة الجمعيات" : string.Empty);

        var userClaims = await _userManager.GetClaimsAsync(user);
        var permissions = (role == "Admin" || role == "CharityManager")
            ? Charity.Domain.Security.AppPermissions.All.ToList()
            : userClaims.Where(c => c.Type == "permission").Select(c => c.Value).Distinct().ToList();

        return new CurrentUserDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            FullName = user.FullName,
            CharityId = user.CharityId ?? string.Empty,
            CharityName = charityName,
            Role = role,
            Permissions = permissions
        };
    }

    private string GenerateJwtToken(ApplicationUser user, string charityName, string role, IEnumerable<string> permissions)
    {
        var jwtKey = _config["Jwt:Key"] ?? "CharitySuperSecretKeyForJwtAuthentication2026!#$";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("FullName", user.FullName ?? string.Empty),
            new("CharityId", user.CharityId ?? string.Empty),
            new("CharityName", charityName),
            new("security_stamp", user.SecurityStamp ?? string.Empty),
            new(ClaimTypes.Role, role)
        };

        foreach (var perm in permissions)
        {
            claims.Add(new Claim("permission", perm));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "CharityApi",
            audience: _config["Jwt:Audience"] ?? "CharityFrontend",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
