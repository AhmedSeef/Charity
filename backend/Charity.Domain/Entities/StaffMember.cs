namespace Charity.Domain.Entities;

public class StaffMember : BaseEntity
{
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Type { get; set; } = "متطوع"; // "متطوع" | "موظف براتب"
    public string Role { get; set; } = "باحث اجتماعي";
    public decimal? Salary { get; set; }
    public string JoinDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Address { get; set; } = string.Empty;
    public string Tasks { get; set; } = string.Empty;
    public string Status { get; set; } = "نشط على رأس العمل";

    // Tenant Isolation
    public string CharityId { get; set; } = string.Empty;
    public Charity? Charity { get; set; }
}
