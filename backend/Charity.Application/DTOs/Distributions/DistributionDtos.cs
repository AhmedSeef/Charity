namespace Charity.Application.DTOs.Distributions;

public class DistributionDto
{
    public string Id { get; set; } = string.Empty;
    public string BeneficiaryId { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string? InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس";
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // تفاصيل المستلم الفعلي للمساعدة
    public string? ReceivedByMemberId { get; set; }
    public string? ReceivedByName { get; set; }
    public string? ReceivedByNationalId { get; set; }
    public string? ReceivedByRelationship { get; set; }

    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;
    public bool IsFromCurrentCharity { get; set; }
}

public class CreateDistributionDto
{
    public string BeneficiaryId { get; set; } = string.Empty;
    public string? InventoryItemId { get; set; }
    public string? ItemName { get; set; }
    public string? Kind { get; set; }
    public double Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public decimal? Amount { get; set; }
    public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public string Notes { get; set; } = string.Empty;

    // تفاصيل المستلم الفعلي للمساعدة
    public string? ReceivedByMemberId { get; set; }
    public string? ReceivedByName { get; set; }
    public string? ReceivedByNationalId { get; set; }
    public string? ReceivedByRelationship { get; set; }
}

public class BeneficiaryAidSummaryDto
{
    public string BeneficiaryId { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public int TotalDistributionsCount { get; set; }
    public int OtherCharitiesDistributionsCount { get; set; }
    public bool HasOtherCharitiesAid => OtherCharitiesDistributionsCount > 0;
    public IReadOnlyList<DistributionDto> AidHistory { get; set; } = new List<DistributionDto>();
}
