namespace Charity.Application.DTOs.Staff;

using Charity.Domain.Validation;

public class StaffMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Type { get; set; } = "متطوع"; // "متطوع" | "موظف براتب"
    public string Role { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public string JoinDate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Tasks { get; set; } = string.Empty;
    public string Status { get; set; } = "نشط على رأس العمل";
    public string CharityId { get; set; } = string.Empty;
}

public class CreateStaffMemberDto
{
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Type { get; set; } = "متطوع";
    public string Role { get; set; } = "باحث اجتماعي";
    public decimal? Salary { get; set; }
    public string JoinDate { get; set; } = BusinessDate.TodayInCairo().ToString("yyyy-MM-dd");
    public string Address { get; set; } = string.Empty;
    public string Tasks { get; set; } = string.Empty;
}

public class UpdateStaffMemberDto
{
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Type { get; set; } = "متطوع";
    public string Role { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public string JoinDate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Tasks { get; set; } = string.Empty;
    public string Status { get; set; } = "نشط على رأس العمل";
}
