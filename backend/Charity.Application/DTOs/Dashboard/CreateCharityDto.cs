using System.ComponentModel.DataAnnotations;

namespace Charity.Application.DTOs.Dashboard;

public sealed class CreateCharityDto
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(500)] public string RoleDesc { get; set; } = "";
    [MaxLength(50)] public string Phone { get; set; } = "";
    [MaxLength(300)] public string Address { get; set; } = "";
    [MaxLength(100)] public string License { get; set; } = "";
    [MaxLength(50)] public string GovernorateId { get; set; } = "";
    [MaxLength(150)] public string GovernorateName { get; set; } = "";
    [MaxLength(50)] public string DistrictId { get; set; } = "";
    [MaxLength(150)] public string DistrictName { get; set; } = "";
    [Required, MinLength(3)] public string ManagerUsername { get; set; } = "";
    [Required, MinLength(6)] public string ManagerPassword { get; set; } = "";
    [Required, MaxLength(200)] public string ManagerFullName { get; set; } = "";
}

public sealed class CreateCharityManagerDto
{
    [Required, MinLength(3)] public string Username { get; set; } = "";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    [Required, MaxLength(200)] public string FullName { get; set; } = "";
}
