namespace Charity.Application.DTOs.Analytics;

public class BiAnalyticsDto
{
    public string CharityId { get; set; } = string.Empty;
    public string CharityName { get; set; } = string.Empty;

    // 1. Executive KPI Summary
    public int RegisteredBeneficiariesCount { get; set; }
    public int TotalBeneficiariesInNetwork { get; set; }
    public double RegisteredSharePercentage { get; set; } // % of total network
    public int TotalDirectFamilyReach { get; set; } // Principal + all family members
    public int ActiveBeneficiariesCount { get; set; }
    public int SuspendedBeneficiariesCount { get; set; }
    public double ActiveRatio { get; set; }

    public int TotalDistributionsCount { get; set; }
    public double TotalUnitsDistributed { get; set; }
    public int PartnerCasesServedCount { get; set; } // Cases registered by other charities served by this charity
    public double FederatedCooperationIndex { get; set; } // % of aid given to partner charities cases

    public decimal TotalCashDonations { get; set; }
    public int InKindDonationsCount { get; set; }
    public decimal AverageCashDonation { get; set; }

    public int TotalStaffCount { get; set; }
    public int VolunteersCount { get; set; }
    public int SalariedStaffCount { get; set; }
    public double VolunteerismRatio { get; set; }

    // 2. Vulnerability & Social Demographics Breakdown
    public List<CategoryMetricDto> CategoryBreakdown { get; set; } = new();
    public List<IncomeMetricDto> IncomeBreakdown { get; set; } = new();
    public List<HousingMetricDto> HousingBreakdown { get; set; } = new();

    // 3. Health & Medical Observatory
    public int ChronicDiseaseCount { get; set; }
    public double ChronicDiseasePercentage { get; set; }
    public int PeriodicMedicationCount { get; set; }
    public List<MedicationFrequencyMetricDto> MedicationFrequencies { get; set; } = new();

    // 4. Family Structure & Dependency
    public int TotalDependentsCount { get; set; } // المعالين
    public int TotalSupportersCount { get; set; } // المعيلين
    public double DependencyRatio { get; set; } // المعالين / المعيلين
    public double AverageFamilySize { get; set; }
    public List<SupportTypeMetricDto> FamilySupportTypes { get; set; } = new();

    // 5. Inventory & Distribution Velocity
    public List<TopDistributedItemDto> TopDistributedItems { get; set; } = new();
    public List<MonthlyVelocityMetricDto> MonthlyTrends { get; set; } = new();

    // 6. Suspension & Attrition Reasons
    public List<SuspensionReasonMetricDto> SuspensionReasonsBreakdown { get; set; } = new();

    // 7. Field Social Research & Multidimensional Poverty Observatory
    public List<VillageMetricDto> VillageHotspots { get; set; } = new();
    public List<PriorityTierMetricDto> PriorityTiersBreakdown { get; set; } = new();
    public List<RoofTypeMetricDto> RoofTypeBreakdown { get; set; } = new();
    public List<InfrastructureDeficitDto> InfrastructureDeficits { get; set; } = new();
    public int TotalInDebtCount { get; set; }
    public decimal TotalDebtBurden { get; set; }
    public int ThreatenedWithPrisonCount { get; set; }
    public int SchoolDropoutsCount { get; set; }
    public int ChildLaborCount { get; set; }
    public int MicroProjectCandidatesCount { get; set; }
    public List<MicroProjectSuggestionDto> TopSuggestedMicroProjects { get; set; } = new();
}

public class VillageMetricDto
{
    public string Village { get; set; } = string.Empty;
    public int BeneficiaryCount { get; set; }
    public int UrgentPriorityCount { get; set; } // أولوية قصوى أ
    public double Percentage { get; set; }
}

public class PriorityTierMetricDto
{
    public string Tier { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class RoofTypeMetricDto
{
    public string RoofType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class InfrastructureDeficitDto
{
    public string DeficitType { get; set; } = string.Empty;
    public int AffectedCount { get; set; }
    public double Percentage { get; set; }
}

public class MicroProjectSuggestionDto
{
    public string ProjectType { get; set; } = string.Empty;
    public int CandidatesCount { get; set; }
}

public class CategoryMetricDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class IncomeMetricDto
{
    public string Bracket { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class HousingMetricDto
{
    public string HousingType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class MedicationFrequencyMetricDto
{
    public string Frequency { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SupportTypeMetricDto
{
    public string SupportType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopDistributedItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public double QuantityDistributed { get; set; }
    public double CurrentStock { get; set; }
}

public class MonthlyVelocityMetricDto
{
    public string MonthName { get; set; } = string.Empty;
    public int DistributionsCount { get; set; }
    public decimal DonationsTotal { get; set; }
}

public class SuspensionReasonMetricDto
{
    public string ReasonTitle { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
