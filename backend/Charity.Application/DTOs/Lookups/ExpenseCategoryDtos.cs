namespace Charity.Application.DTOs.Lookups;

public class ExpenseCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? CharityId { get; set; }
    public string? CharityName { get; set; }

    // إجماليات مالية وحركات الصرف
    public decimal TotalDisbursedAmount { get; set; }
    public int DisbursementsCount { get; set; }
    public int BeneficiariesCount { get; set; }
    public List<string> BeneficiaryIds { get; set; } = new();
    public bool HasHistoricalDisbursements { get; set; }
    public bool CanDelete => !HasHistoricalDisbursements;
}

public class CreateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public class UpdateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ExpenseCategoryDisbursementDetailDto
{
    public string DistributionId { get; set; } = string.Empty;
    public string BeneficiaryId { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string FamilyCode { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? ReceivedByName { get; set; }
    public string? ReceivedByRelationship { get; set; }
    public string? ReceivedByNationalId { get; set; }
    public string CharityName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class MonthlyExpenseMovementDto
{
    public string MonthYear { get; set; } = string.Empty; // e.g. "2026-10"
    public string MonthLabel { get; set; } = string.Empty; // e.g. "شهر 10/2026"
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public int DisbursementsCount { get; set; }
    public int BeneficiariesCount { get; set; }
    public List<string> BeneficiaryIds { get; set; } = new();
    public List<CategoryMonthlyShareDto> TopCategories { get; set; } = new();
}

public class CategoryMonthlyShareDto
{
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DisbursementsCount { get; set; }
}
