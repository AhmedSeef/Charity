using Charity.Application.DTOs.Analytics;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class BiAnalyticsService : IBiAnalyticsService
{
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly IRepository<Distribution> _distributionRepo;
    private readonly IRepository<Donation> _donationRepo;
    private readonly IRepository<InventoryItem> _inventoryRepo;
    private readonly IRepository<StaffMember> _staffRepo;
    private readonly IRepository<Charity.Domain.Entities.Charity> _charityRepo;
    private readonly ICurrentUserService _currentUserService;

    public BiAnalyticsService(
        IRepository<Beneficiary> beneficiaryRepo,
        IRepository<Distribution> distributionRepo,
        IRepository<Donation> donationRepo,
        IRepository<InventoryItem> inventoryRepo,
        IRepository<StaffMember> staffRepo,
        IRepository<Charity.Domain.Entities.Charity> charityRepo,
        ICurrentUserService currentUserService)
    {
        _beneficiaryRepo = beneficiaryRepo;
        _distributionRepo = distributionRepo;
        _donationRepo = donationRepo;
        _inventoryRepo = inventoryRepo;
        _staffRepo = staffRepo;
        _charityRepo = charityRepo;
        _currentUserService = currentUserService;
    }

    public async Task<BiAnalyticsDto> GetCharityAnalyticsAsync(string? targetCharityId = null)
    {
        var charityId = !string.IsNullOrWhiteSpace(targetCharityId) 
            ? targetCharityId 
            : (_currentUserService.CharityId ?? "c1");

        var charity = await _charityRepo.GetByIdAsync(charityId);
        var charityName = charity?.Name ?? _currentUserService.CharityName ?? "جمعيتكم";

        // Load network datasets
        var allBeneficiaries = (await _beneficiaryRepo.GetAllAsync()).ToList();
        var allDistributions = (await _distributionRepo.GetAllAsync()).ToList();

        // Filter tenant-specific datasets
        var myRegisteredBeneficiaries = allBeneficiaries
            .Where(b => b.RegisteredByCharityId == charityId || string.IsNullOrEmpty(b.RegisteredByCharityId))
            .ToList();

        var myDistributions = allDistributions
            .Where(d => d.CharityId == charityId)
            .ToList();

        var myDonations = (await _donationRepo.FindAsync(d => d.CharityId == charityId)).ToList();
        var myInventory = (await _inventoryRepo.FindAsync(i => i.CharityId == charityId)).ToList();
        var myStaff = (await _staffRepo.FindAsync(s => s.CharityId == charityId)).ToList();

        // 1. Executive KPIs
        var registeredCount = myRegisteredBeneficiaries.Count;
        var totalNetworkCount = allBeneficiaries.Count;
        var registeredShare = totalNetworkCount > 0 
            ? Math.Round((double)registeredCount / totalNetworkCount * 100, 1) 
            : 0;

        var activeCount = myRegisteredBeneficiaries.Count(b => !b.IsSuspended && b.Status != "موقوف");
        var suspendedCount = myRegisteredBeneficiaries.Count(b => b.IsSuspended || b.Status == "موقوف");
        var activeRatio = registeredCount > 0 
            ? Math.Round((double)activeCount / registeredCount * 100, 1) 
            : 0;

        // Family Reach: Principal + Dependents/Supporters in FamilyMembers
        var totalFamilyReach = myRegisteredBeneficiaries.Sum(b => 
            Math.Max(1, (b.FamilyMembers?.Count ?? 0) + 1));

        // Federated Collaboration: Distributions delivered by this charity to beneficiaries registered by other charities!
        var partnerCasesServed = myDistributions
            .Where(d => {
                var ben = allBeneficiaries.FirstOrDefault(b => b.Id == d.BeneficiaryId);
                return ben != null && !string.IsNullOrEmpty(ben.RegisteredByCharityId) && ben.RegisteredByCharityId != charityId;
            })
            .Select(d => d.BeneficiaryId)
            .Distinct()
            .Count();

        var totalDistCount = myDistributions.Count;
        var totalUnitsDistributed = myDistributions.Sum(d => d.Quantity);
        var federatedCooperationIndex = totalDistCount > 0 
            ? Math.Round((double)partnerCasesServed / totalDistCount * 100, 1) 
            : 0;

        // Cash & In-Kind Donations
        var cashDonations = myDonations.Where(d => d.DonationKind == "نقدي" || string.IsNullOrEmpty(d.DonationKind)).ToList();
        var totalCash = cashDonations.Sum(d => d.Amount ?? 0);
        var inKindCount = myDonations.Count(d => d.DonationKind == "عيني");
        var avgDonation = cashDonations.Count > 0 ? Math.Round(totalCash / cashDonations.Count, 0) : 0;

        // HR & Volunteerism
        var totalStaff = myStaff.Count;
        var volunteers = myStaff.Count(s => s.Role?.Contains("متطوع") == true || s.Salary == null || s.Salary == 0);
        var salaried = totalStaff - volunteers;
        var volunteerRatio = totalStaff > 0 ? Math.Round((double)volunteers / totalStaff * 100, 1) : 0;

        // 2. Vulnerability Breakdown (Categories, Income, Housing)
        var categoryGroups = myRegisteredBeneficiaries
            .GroupBy(b => string.IsNullOrWhiteSpace(b.Category) ? "أخرى" : b.Category)
            .Select(g => new CategoryMetricDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = registeredCount > 0 ? Math.Round((double)g.Count() / registeredCount * 100, 1) : 0
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        var incomeGroups = myRegisteredBeneficiaries
            .GroupBy(b => {
                if (!b.MonthlyIncome.HasValue || b.MonthlyIncome == 0) return "بدون دخل ثابت (يومية/حرج)";
                if (b.MonthlyIncome <= 2000) return "أقل من 2000 ج.م";
                if (b.MonthlyIncome <= 4000) return "2000 - 4000 ج.م";
                return "أكثر من 4000 ج.م";
            })
            .Select(g => new IncomeMetricDto
            {
                Bracket = g.Key,
                Count = g.Count(),
                Percentage = registeredCount > 0 ? Math.Round((double)g.Count() / registeredCount * 100, 1) : 0
            })
            .OrderByDescending(i => i.Count)
            .ToList();

        var housingGroups = myRegisteredBeneficiaries
            .GroupBy(b => string.IsNullOrWhiteSpace(b.Housing) ? "غير محدد" : b.Housing)
            .Select(g => new HousingMetricDto
            {
                HousingType = g.Key,
                Count = g.Count(),
                Percentage = registeredCount > 0 ? Math.Round((double)g.Count() / registeredCount * 100, 1) : 0
            })
            .OrderByDescending(h => h.Count)
            .ToList();

        // 3. Health & Medical Observatory
        var chronicCount = myRegisteredBeneficiaries.Count(b => b.HasChronicDisease);
        var chronicPercentage = registeredCount > 0 ? Math.Round((double)chronicCount / registeredCount * 100, 1) : 0;
        var periodicMedCount = myRegisteredBeneficiaries.Count(b => b.NeedsPeriodicMedication);

        var medFreqs = myRegisteredBeneficiaries
            .Where(b => b.NeedsPeriodicMedication && !string.IsNullOrWhiteSpace(b.MedicationFrequency))
            .GroupBy(b => b.MedicationFrequency!)
            .Select(g => new MedicationFrequencyMetricDto
            {
                Frequency = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(f => f.Count)
            .ToList();

        // 4. Family Structure & Support Types
        var allFamilyMembers = myRegisteredBeneficiaries
            .SelectMany(b => b.FamilyMembers ?? new List<BeneficiaryFamilyMember>())
            .ToList();

        var dependents = allFamilyMembers.Count(m => m.MemberType == "معال" || string.IsNullOrEmpty(m.MemberType));
        var supporters = allFamilyMembers.Count(m => m.MemberType == "معيل");
        var depRatio = supporters > 0 ? Math.Round((double)dependents / supporters, 1) : dependents;
        var avgFamilySize = registeredCount > 0 
            ? Math.Round(myRegisteredBeneficiaries.Average(b => b.FamilyCount > 0 ? b.FamilyCount : 1), 1) 
            : 0;

        var supportTypes = allFamilyMembers
            .GroupBy(m => string.IsNullOrWhiteSpace(m.SupportType) ? "أخرى" : m.SupportType)
            .Select(g => new SupportTypeMetricDto
            {
                SupportType = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        // 5. Inventory & Distribution Velocity
        var topItems = myDistributions
            .GroupBy(d => d.ItemName ?? "مساعدة")
            .Select(g => {
                var invItem = myInventory.FirstOrDefault(i => i.ItemName == g.Key);
                return new TopDistributedItemDto
                {
                    ItemName = g.Key,
                    Unit = invItem?.Unit ?? "وحدة",
                    Kind = invItem?.Kind ?? "صنف ملموس",
                    QuantityDistributed = g.Sum(x => x.Quantity),
                    CurrentStock = invItem?.Quantity ?? 0
                };
            })
            .OrderByDescending(t => t.QuantityDistributed)
            .Take(6)
            .ToList();

        // Monthly Trends (Last 6 months)
        var monthlyTrends = myDistributions
            .GroupBy(d => !string.IsNullOrEmpty(d.Date) && d.Date.Length >= 7 ? d.Date.Substring(0, 7) : "غير محدد")
            .OrderBy(g => g.Key)
            .TakeLast(6)
            .Select(g => {
                var monthDonations = myDonations
                    .Where(d => (!string.IsNullOrEmpty(d.Date) && d.Date.Length >= 7 ? d.Date.Substring(0, 7) : "غير محدد") == g.Key && d.Amount.HasValue)
                    .Sum(d => d.Amount!.Value);

                return new MonthlyVelocityMetricDto
                {
                    MonthName = g.Key,
                    DistributionsCount = g.Count(),
                    DonationsTotal = monthDonations
                };
            })
            .ToList();

        // 6. Suspension Reasons Breakdown
        var suspensionReasons = myRegisteredBeneficiaries
            .Where(b => (b.IsSuspended || b.Status == "موقوف") && !string.IsNullOrWhiteSpace(b.SuspensionReasonTitle))
            .GroupBy(b => b.SuspensionReasonTitle!)
            .Select(g => new SuspensionReasonMetricDto
            {
                ReasonTitle = g.Key,
                Count = g.Count(),
                Percentage = suspendedCount > 0 ? Math.Round((double)g.Count() / suspendedCount * 100, 1) : 0
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        return new BiAnalyticsDto
        {
            CharityId = charityId,
            CharityName = charityName,
            RegisteredBeneficiariesCount = registeredCount,
            TotalBeneficiariesInNetwork = totalNetworkCount,
            RegisteredSharePercentage = registeredShare,
            TotalDirectFamilyReach = totalFamilyReach,
            ActiveBeneficiariesCount = activeCount,
            SuspendedBeneficiariesCount = suspendedCount,
            ActiveRatio = activeRatio,
            TotalDistributionsCount = totalDistCount,
            TotalUnitsDistributed = totalUnitsDistributed,
            PartnerCasesServedCount = partnerCasesServed,
            FederatedCooperationIndex = federatedCooperationIndex,
            TotalCashDonations = totalCash,
            InKindDonationsCount = inKindCount,
            AverageCashDonation = avgDonation,
            TotalStaffCount = totalStaff,
            VolunteersCount = volunteers,
            SalariedStaffCount = salaried,
            VolunteerismRatio = volunteerRatio,
            CategoryBreakdown = categoryGroups,
            IncomeBreakdown = incomeGroups,
            HousingBreakdown = housingGroups,
            ChronicDiseaseCount = chronicCount,
            ChronicDiseasePercentage = chronicPercentage,
            PeriodicMedicationCount = periodicMedCount,
            MedicationFrequencies = medFreqs,
            TotalDependentsCount = dependents,
            TotalSupportersCount = supporters,
            DependencyRatio = depRatio,
            AverageFamilySize = avgFamilySize,
            FamilySupportTypes = supportTypes,
            TopDistributedItems = topItems,
            MonthlyTrends = monthlyTrends,
            SuspensionReasonsBreakdown = suspensionReasons
        };
    }
}
