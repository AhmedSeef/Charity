namespace Charity.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    // Active charity metrics (tenant isolated)
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public decimal TotalCashDonations { get; set; }
    public int InKindDonationsCount { get; set; }
    public int TangibleItemsCount { get; set; }
    public int ServicesCount { get; set; }
    public int DistributionsCount { get; set; }
    public int StaffCount { get; set; }

    // Shared Federation Network Metrics
    public int TotalNetworkBeneficiariesCount { get; set; }
    public int TotalPartnerCharitiesCount { get; set; }
    public int TotalNetworkDistributionsCount { get; set; }
}

public class CharityDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoleDesc { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
}

public class CharitySettingsDto
{
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
