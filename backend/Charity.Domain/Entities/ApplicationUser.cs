using Microsoft.AspNetCore.Identity;

namespace Charity.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string CharityId { get; set; } = string.Empty;
    public Charity? Charity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
