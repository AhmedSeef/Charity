namespace Charity.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class CurrentUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
