using Charity.Application.DTOs.Auth;

namespace Charity.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<CurrentUserDto?> GetCurrentUserAsync();
}
