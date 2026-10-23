namespace Charity.Application.DTOs.Distributions;

using Charity.Domain.Validation;

public class DistributionDto
{
    public List<string> Warnings { get; set; } = new();
    public double? RemainingStock { get; set; }
    public decimal? RemainingCashBalance { get; set; }
    public string Id { get; set; } = string.Empty;
    public string BeneficiaryId { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string? InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Kind { get; set; } = "صنف ملموس";
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? ExpenseCategoryId { get; set; }
    public string? ExpenseCategoryName { get; set; }
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
    public bool AllowNegativeBalance { get; set; }
    public string BeneficiaryId { get; set; } = string.Empty;
    public string? InventoryItemId { get; set; }
    public string? ItemName { get; set; }
    public string? Kind { get; set; }
    public double Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public decimal? Amount { get; set; }
    public string? ExpenseCategoryId { get; set; }
    public string? ExpenseCategoryName { get; set; }
    public string Date { get; set; } = BusinessDate.TodayInCairo().ToString("yyyy-MM-dd");
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

public class BulkDistributionRequestDto
{
    public bool AllowNegativeBalance { get; set; }
    public List<string> BeneficiaryIds { get; set; } = new();
    public string? InventoryItemId { get; set; }
    public string? ItemName { get; set; }
    public string Kind { get; set; } = "صنف ملموس"; // "صنف ملموس" | "إعانة نقدية" | "خدمة"
    public double QuantityPerBeneficiary { get; set; } = 1;
    public string? Unit { get; set; }
    public decimal? AmountPerBeneficiary { get; set; }
    public string? ExpenseCategoryId { get; set; }
    public string? ExpenseCategoryName { get; set; }
    public string Date { get; set; } = BusinessDate.TodayInCairo().ToString("yyyy-MM-dd");
    public string Notes { get; set; } = string.Empty;

    // بيانات خط السير الميداني والتكليف
    public string? VolunteerOrWorkerName { get; set; }
    public string? VolunteerPhone { get; set; }
    public string? TargetAreaName { get; set; }
    public string? TargetAreaFilter { get; set; }
    public string? TargetGovernorateId { get; set; }
    public string? TargetDistrictId { get; set; }
    public string? TargetPriorityTier { get; set; }
}

public class BulkDistributionResultDto
{
    public List<string> Warnings { get; set; } = new();
    public decimal? RemainingCashBalance { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public double TotalQuantityDisbursed { get; set; }
    public decimal TotalAmountDisbursed { get; set; }
    public string? InventoryItemName { get; set; }
    public double RemainingStock { get; set; }
    public string? VolunteerOrWorkerName { get; set; }
    public string? VolunteerPhone { get; set; }
    public string? TargetAreaName { get; set; }
    public string Date { get; set; } = string.Empty;
    public List<string> SkippedBeneficiaryNames { get; set; } = new();
    public List<DistributionDto> CreatedDistributions { get; set; } = new();
}
