namespace Charity.Application.DTOs.Staff;

public class StaffUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CharityId { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff";
    public string RoleTemplate { get; set; } = "Custom";
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByName { get; set; }
}

public class CreateStaffUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleTemplate { get; set; } = "Custom";
    public List<string> Permissions { get; set; } = new();
    public string? StaffMemberId { get; set; }
}

public class UpdateStaffUserPermissionsDto
{
    public string RoleTemplate { get; set; } = "Custom";
    public List<string> Permissions { get; set; } = new();
}

public class ChangeStaffUserPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
