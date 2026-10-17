using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Dashboard;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Donations;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Staff;
using Charity.Application.DTOs.SuspensionReasons;
using Charity.Domain.Entities;

namespace Charity.Application.Mappers;

public static class EntityMappingExtensions
{
    // SuspensionReason
    public static SuspensionReasonDto ToDto(this SuspensionReason r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        IsActive = r.IsActive,
        CharityId = r.CharityId,
        CharityName = r.CharityName
    };

    // Charity
    public static CharityDto ToDto(this Charity.Domain.Entities.Charity c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        RoleDesc = c.RoleDesc,
        Logo = c.Logo,
        Color = c.Color,
        Icon = c.Icon,
        Phone = c.Phone,
        Address = c.Address,
        License = c.License
    };

    // FamilyMember
    public static FamilyMemberDto ToDto(this BeneficiaryFamilyMember m) => new()
    {
        Id = m.Id,
        BeneficiaryId = m.BeneficiaryId,
        NationalId = m.NationalId,
        Name = m.Name,
        Relationship = m.Relationship,
        MemberType = m.MemberType,
        HasIncome = m.HasIncome,
        IncomeAmount = m.IncomeAmount,
        IncomeSource = m.IncomeSource,
        SupportType = m.SupportType,
        HealthStatus = m.HealthStatus,
        Notes = m.Notes,
        EducationStage = m.EducationStage,
        IsSchoolDropout = m.IsSchoolDropout,
        IsChildLabor = m.IsChildLabor,
        IsSeparated = m.IsSeparated,
        SeparatedAt = m.SeparatedAt,
        SeparatedToBeneficiaryId = m.SeparatedToBeneficiaryId,
        SeparationReason = m.SeparationReason,
        IsFormerHead = m.IsFormerHead
    };

    // BeneficiaryHeadHistory
    public static BeneficiaryHeadHistoryDto ToDto(this BeneficiaryHeadHistory h) => new()
    {
        Id = h.Id,
        BeneficiaryId = h.BeneficiaryId,
        PreviousHeadName = h.PreviousHeadName,
        PreviousHeadNationalId = h.PreviousHeadNationalId,
        NewHeadName = h.NewHeadName,
        NewHeadNationalId = h.NewHeadNationalId,
        Reason = h.Reason,
        Notes = h.Notes,
        TransferredAt = h.TransferredAt,
        TransferredByCharityName = h.TransferredByCharityName
    };

    public static BeneficiaryFamilyMember ToEntity(this CreateFamilyMemberDto dto, string beneficiaryId) => new()
    {
        Id = string.IsNullOrEmpty(dto.Id) ? Guid.NewGuid().ToString() : dto.Id,
        BeneficiaryId = beneficiaryId,
        NationalId = dto.NationalId.Trim(),
        Name = dto.Name.Trim(),
        Relationship = dto.Relationship,
        MemberType = string.IsNullOrEmpty(dto.MemberType) ? "معال" : dto.MemberType,
        HasIncome = dto.HasIncome,
        IncomeAmount = dto.IncomeAmount,
        IncomeSource = dto.IncomeSource ?? string.Empty,
        SupportType = dto.SupportType ?? "طالب",
        HealthStatus = dto.HealthStatus ?? string.Empty,
        Notes = dto.Notes ?? string.Empty,
        EducationStage = dto.EducationStage ?? "منتظم بالتعليم",
        IsSchoolDropout = dto.IsSchoolDropout,
        IsChildLabor = dto.IsChildLabor
    };

    // Beneficiary
    public static BeneficiaryDto ToDto(this Beneficiary b, string currentCharityId) => new()
    {
        Id = b.Id,
        FamilyCode = b.FamilyCode,
        FamilyName = b.FamilyName,
        NationalId = b.NationalId,
        Name = b.Name,
        Phone = b.Phone,
        Category = b.Category,
        MaritalStatus = b.MaritalStatus,
        FamilyCount = b.FamilyCount,
        Address = b.Address,
        Housing = b.Housing,
        HealthIssues = b.HealthIssues,
        ResearchNotes = b.ResearchNotes,
        Status = b.Status,
        IncomeSource = b.IncomeSource,
        MonthlyIncome = b.MonthlyIncome,
        HasLandOrProperty = b.HasLandOrProperty,
        LandOrPropertyDetails = b.LandOrPropertyDetails,
        HasGovernmentPension = b.HasGovernmentPension,
        GovernmentPensionDetails = b.GovernmentPensionDetails,
        HasChronicDisease = b.HasChronicDisease,
        ChronicDiseaseDetails = b.ChronicDiseaseDetails,
        NeedsPeriodicMedication = b.NeedsPeriodicMedication,
        PeriodicMedicationDetails = b.PeriodicMedicationDetails,
        MedicationFrequency = b.MedicationFrequency,
        VillageOrDistrict = b.VillageOrDistrict,
        PriorityTier = b.PriorityTier,
        RoofType = b.RoofType,
        WallType = b.WallType,
        WaterSource = b.WaterSource,
        SanitationType = b.SanitationType,
        ElectricitySource = b.ElectricitySource,
        AppliancesSummary = b.AppliancesSummary,
        HasDebts = b.HasDebts,
        TotalDebtAmount = b.TotalDebtAmount,
        DebtCreditor = b.DebtCreditor,
        IsThreatenedWithImprisonment = b.IsThreatenedWithImprisonment,
        DebtNotes = b.DebtNotes,
        IsCandidateForMicroProject = b.IsCandidateForMicroProject,
        SuggestedMicroProject = b.SuggestedMicroProject,
        IsSuspended = b.IsSuspended || b.Status == "موقوف",
        SuspensionReasonId = b.SuspensionReasonId,
        SuspensionReasonTitle = b.SuspensionReasonTitle,
        SuspensionNotes = b.SuspensionNotes,
        SuspendedAt = b.SuspendedAt,
        SuspendedByCharityId = b.SuspendedByCharityId,
        SuspendedByCharityName = b.SuspendedByCharityName,
        RegisteredByCharityId = b.RegisteredByCharityId,
        RegisteredByCharityName = b.RegisteredByCharityName,
        IsEditableByCurrentCharity = string.IsNullOrEmpty(b.RegisteredByCharityId) || b.RegisteredByCharityId == currentCharityId,
        FamilyMembers = b.FamilyMembers?.Select(m => m.ToDto()).ToList() ?? new(),
        HeadHistories = b.HeadHistories?.Select(h => h.ToDto()).OrderByDescending(h => h.TransferredAt).ToList() ?? new()
    };

    public static Beneficiary ToEntity(this CreateBeneficiaryDto dto, string charityId, string charityName) => new()
    {
        FamilyCode = !string.IsNullOrWhiteSpace(dto.FamilyCode) 
            ? dto.FamilyCode.Trim() 
            : string.Empty,
        FamilyName = string.IsNullOrWhiteSpace(dto.FamilyName) 
            ? $"عائلة {dto.Name.Trim()}" 
            : dto.FamilyName.Trim(),
        NationalId = dto.NationalId.Trim(),
        Name = dto.Name.Trim(),
        Phone = dto.Phone,
        Category = dto.Category,
        MaritalStatus = dto.MaritalStatus,
        FamilyCount = dto.FamilyCount,
        Address = dto.Address,
        Housing = dto.Housing,
        HealthIssues = dto.HealthIssues,
        ResearchNotes = dto.ResearchNotes,
        Status = dto.Status,
        IncomeSource = dto.IncomeSource ?? string.Empty,
        MonthlyIncome = dto.MonthlyIncome,
        HasLandOrProperty = dto.HasLandOrProperty,
        LandOrPropertyDetails = dto.LandOrPropertyDetails ?? string.Empty,
        HasGovernmentPension = dto.HasGovernmentPension,
        GovernmentPensionDetails = dto.GovernmentPensionDetails ?? string.Empty,
        HasChronicDisease = dto.HasChronicDisease,
        ChronicDiseaseDetails = dto.ChronicDiseaseDetails ?? string.Empty,
        NeedsPeriodicMedication = dto.NeedsPeriodicMedication,
        PeriodicMedicationDetails = dto.PeriodicMedicationDetails ?? string.Empty,
        MedicationFrequency = dto.MedicationFrequency ?? string.Empty,
        VillageOrDistrict = dto.VillageOrDistrict ?? string.Empty,
        PriorityTier = string.IsNullOrEmpty(dto.PriorityTier) ? "أ - أولوية قصوى ومعدم" : dto.PriorityTier,
        RoofType = string.IsNullOrEmpty(dto.RoofType) ? "خرساني مسلح" : dto.RoofType,
        WallType = string.IsNullOrEmpty(dto.WallType) ? "طوب أحمر ومحارة" : dto.WallType,
        WaterSource = string.IsNullOrEmpty(dto.WaterSource) ? "عداد مياه رسمي" : dto.WaterSource,
        SanitationType = string.IsNullOrEmpty(dto.SanitationType) ? "شبكة صرف صحي عمومية" : dto.SanitationType,
        ElectricitySource = string.IsNullOrEmpty(dto.ElectricitySource) ? "عداد قانوني" : dto.ElectricitySource,
        AppliancesSummary = dto.AppliancesSummary ?? string.Empty,
        HasDebts = dto.HasDebts,
        TotalDebtAmount = dto.TotalDebtAmount,
        DebtCreditor = dto.DebtCreditor ?? string.Empty,
        IsThreatenedWithImprisonment = dto.IsThreatenedWithImprisonment,
        DebtNotes = dto.DebtNotes ?? string.Empty,
        IsCandidateForMicroProject = dto.IsCandidateForMicroProject,
        SuggestedMicroProject = dto.SuggestedMicroProject ?? string.Empty,
        RegisteredByCharityId = charityId,
        RegisteredByCharityName = charityName
    };

    // Inventory
    public static InventoryItemDto ToDto(this InventoryItem i) => new()
    {
        Id = i.Id,
        Code = i.Code,
        ItemName = i.ItemName,
        Kind = i.Kind,
        Unit = i.Unit,
        Type = i.Type,
        Quantity = i.Quantity,
        MinAlert = i.MinAlert,
        Notes = i.Notes,
        CharityId = i.CharityId
    };

    public static InventoryItem ToEntity(this CreateInventoryItemDto dto, string charityId) => new()
    {
        Code = string.IsNullOrWhiteSpace(dto.Code) 
            ? (dto.Kind == "خدمة" ? "SRV-" : "ITM-") + Random.Shared.Next(100, 999) 
            : dto.Code,
        ItemName = dto.ItemName,
        Kind = dto.Kind,
        Unit = dto.Unit,
        Type = dto.Type,
        Quantity = dto.Kind == "خدمة" ? 0 : dto.Quantity,
        MinAlert = dto.MinAlert,
        Notes = dto.Notes,
        CharityId = charityId
    };

    // Donation
    public static DonationDto ToDto(this Donation d) => new()
    {
        Id = d.Id,
        DonorName = d.DonorName,
        DonorPhone = d.DonorPhone,
        DonationKind = d.DonationKind,
        Amount = d.Amount,
        CashCategory = d.CashCategory,
        PaymentMethod = d.PaymentMethod,
        InventoryItemId = d.InventoryItemId,
        ItemName = d.ItemName,
        Quantity = d.Quantity,
        Unit = d.Unit,
        EstimatedValue = d.EstimatedValue,
        Date = d.Date,
        Notes = d.Notes,
        CharityId = d.CharityId
    };

    public static Donation ToEntity(this CreateDonationDto dto, string charityId) => new()
    {
        DonorName = dto.DonorName,
        DonorPhone = dto.DonorPhone,
        DonationKind = dto.DonationKind,
        Amount = dto.Amount,
        CashCategory = dto.CashCategory,
        PaymentMethod = dto.PaymentMethod,
        InventoryItemId = dto.InventoryItemId,
        ItemName = dto.ItemName,
        Quantity = dto.Quantity,
        Unit = dto.Unit,
        EstimatedValue = dto.EstimatedValue,
        Date = string.IsNullOrWhiteSpace(dto.Date) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : dto.Date,
        Notes = dto.Notes,
        CharityId = charityId
    };

    // Distribution
    public static DistributionDto ToDto(this Distribution dist, string currentCharityId) => new()
    {
        Id = dist.Id,
        BeneficiaryId = dist.BeneficiaryId,
        BeneficiaryName = dist.Beneficiary?.Name ?? string.Empty,
        InventoryItemId = dist.InventoryItemId,
        ItemName = dist.ItemName,
        Kind = dist.Kind,
        Quantity = dist.Quantity,
        Unit = dist.Unit,
        Amount = dist.Amount,
        Date = dist.Date,
        Notes = dist.Notes,
        ReceivedByMemberId = dist.ReceivedByMemberId,
        ReceivedByName = dist.ReceivedByName,
        ReceivedByNationalId = dist.ReceivedByNationalId,
        ReceivedByRelationship = dist.ReceivedByRelationship,
        CharityId = dist.CharityId,
        CharityName = dist.CharityName,
        IsFromCurrentCharity = dist.CharityId == currentCharityId
    };

    public static Distribution ToEntity(this CreateDistributionDto dto, string charityId, string charityName) => new()
    {
        BeneficiaryId = dto.BeneficiaryId,
        InventoryItemId = dto.InventoryItemId,
        ItemName = dto.ItemName ?? string.Empty,
        Kind = dto.Kind ?? "صنف ملموس",
        Quantity = dto.Quantity,
        Unit = dto.Unit ?? string.Empty,
        Amount = dto.Amount,
        Date = string.IsNullOrWhiteSpace(dto.Date) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : dto.Date,
        Notes = dto.Notes ?? string.Empty,
        ReceivedByMemberId = dto.ReceivedByMemberId,
        ReceivedByName = dto.ReceivedByName,
        ReceivedByNationalId = dto.ReceivedByNationalId,
        ReceivedByRelationship = dto.ReceivedByRelationship,
        CharityId = charityId,
        CharityName = charityName
    };

    // Staff
    public static StaffMemberDto ToDto(this StaffMember s) => new()
    {
        Id = s.Id,
        NationalId = s.NationalId,
        Name = s.Name,
        Phone = s.Phone,
        Type = s.Type,
        Role = s.Role,
        Salary = s.Salary,
        JoinDate = s.JoinDate,
        Address = s.Address,
        Tasks = s.Tasks,
        Status = s.Status,
        CharityId = s.CharityId
    };

    public static StaffMember ToEntity(this CreateStaffMemberDto dto, string charityId) => new()
    {
        NationalId = dto.NationalId,
        Name = dto.Name,
        Phone = dto.Phone,
        Type = dto.Type,
        Role = dto.Role,
        Salary = dto.Type == "متطوع" ? null : dto.Salary,
        JoinDate = string.IsNullOrWhiteSpace(dto.JoinDate) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : dto.JoinDate,
        Address = dto.Address,
        Tasks = dto.Tasks,
        Status = "نشط على رأس العمل",
        CharityId = charityId
    };
}
