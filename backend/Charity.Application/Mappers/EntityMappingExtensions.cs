using Charity.Application.DTOs.Audit;
using Charity.Application.DTOs.Beneficiaries;
using Charity.Application.DTOs.Dashboard;
using Charity.Application.DTOs.Distributions;
using Charity.Application.DTOs.Donations;
using Charity.Application.DTOs.Inventory;
using Charity.Application.DTOs.Staff;
using Charity.Application.DTOs.SuspensionReasons;
using Charity.Application.DTOs.SeparationReasons;
using Charity.Application.DTOs.Lookups;
using Charity.Domain.Entities;

namespace Charity.Application.Mappers;

public static class EntityMappingExtensions
{
    private static T WithAudit<T>(this T dto, BaseEntity entity) where T : AuditedDto
    {
        dto.CreatedAt = entity.CreatedAt;
        dto.CreatedBy = entity.CreatedBy;
        dto.CreatedByName = entity.CreatedByName;
        dto.UpdatedAt = entity.UpdatedAt;
        dto.UpdatedBy = entity.UpdatedBy;
        dto.UpdatedByName = entity.UpdatedByName;
        dto.DeletedAt = entity.DeletedAt;
        dto.DeletedBy = entity.DeletedBy;
        dto.DeletedByName = entity.DeletedByName;
        return dto;
    }

    public static CharitySettingsDto ToDto(this CharitySettings settings) => new CharitySettingsDto
    {
        CharityId = settings.CharityId,
        CharityName = settings.CharityName,
        Logo = settings.Logo,
        Phone = settings.Phone,
        Details = settings.Details
    }.WithAudit(settings);

    // SuspensionReason
    public static SuspensionReasonDto ToDto(this SuspensionReason r) => new SuspensionReasonDto()
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        IsActive = r.IsActive,
        CharityId = r.CharityId ?? string.Empty,
        CharityName = r.CharityName ?? string.Empty
    }.WithAudit(r);

    // SeparationReason
    public static SeparationReasonDto ToDto(this SeparationReason r) => new SeparationReasonDto()
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        IsActive = r.IsActive,
        CharityId = r.CharityId ?? string.Empty,
        CharityName = r.CharityName ?? string.Empty
    }.WithAudit(r);

    // Charity
    public static CharityDto ToDto(this Charity.Domain.Entities.Charity c) => new CharityDto()
    {
        Id = c.Id,
        Name = c.Name,
        RoleDesc = c.RoleDesc,
        Logo = c.Logo,
        Color = c.Color,
        Icon = c.Icon,
        Phone = c.Phone,
        Address = c.Address,
        License = c.License,
        GovernorateId = c.GovernorateId,
        GovernorateName = c.GovernorateName,
        DistrictId = c.DistrictId,
        DistrictName = c.DistrictName
    }.WithAudit(c);

    // FamilyMember
    public static FamilyMemberDto ToDto(this BeneficiaryFamilyMember m) => new FamilyMemberDto()
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
        SeparationReasonId = m.SeparationReasonId,
        SeparationReason = m.SeparationReason,
        IsFormerHead = m.IsFormerHead
    }.WithAudit(m);

    // BeneficiaryHeadHistory
    public static BeneficiaryHeadHistoryDto ToDto(this BeneficiaryHeadHistory h) => new BeneficiaryHeadHistoryDto()
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
    }.WithAudit(h);

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
    public static BeneficiaryDto ToDto(this Beneficiary b, string currentCharityId) => new BeneficiaryDto()
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
        GovernorateId = b.GovernorateId,
        GovernorateName = b.GovernorateName,
        DistrictId = b.DistrictId,
        DistrictName = b.DistrictName,
        VillageId = b.VillageId,
        VillageName = b.VillageName ?? b.Village?.Name,
        VillageAreaId = b.VillageAreaId,
        VillageAreaName = b.VillageAreaName ?? b.VillageArea?.Name,
        AddressId = b.AddressId,
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
        IsEditableByCurrentCharity = !string.IsNullOrWhiteSpace(currentCharityId) && b.RegisteredByCharityId == currentCharityId,
        FamilyMembers = b.FamilyMembers?.Select(m => m.ToDto()).ToList() ?? new(),
        HeadHistories = b.HeadHistories?.Select(h => h.ToDto()).OrderByDescending(h => h.TransferredAt).ToList() ?? new()
    }.WithAudit(b);

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
        GovernorateId = string.IsNullOrWhiteSpace(dto.GovernorateId) ? null : dto.GovernorateId,
        GovernorateName = dto.GovernorateName,
        DistrictId = string.IsNullOrWhiteSpace(dto.DistrictId) ? null : dto.DistrictId,
        DistrictName = dto.DistrictName,
        VillageId = string.IsNullOrWhiteSpace(dto.VillageId) ? null : dto.VillageId,
        VillageName = dto.VillageName,
        VillageAreaId = string.IsNullOrWhiteSpace(dto.VillageAreaId) ? null : dto.VillageAreaId,
        VillageAreaName = dto.VillageAreaName,
        AddressId = string.IsNullOrWhiteSpace(dto.AddressId) ? null : dto.AddressId,
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
    public static InventoryItemDto ToDto(this InventoryItem i) => new InventoryItemDto()
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
    }.WithAudit(i);

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
    public static DonationDto ToDto(this Donation d) => new DonationDto()
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
    }.WithAudit(d);

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
    public static DistributionDto ToDto(this Distribution dist, string currentCharityId) => new DistributionDto()
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
        ExpenseCategoryId = dist.ExpenseCategoryId,
        ExpenseCategoryName = dist.ExpenseCategoryName ?? dist.ExpenseCategory?.Name,
        Date = dist.Date,
        Notes = dist.Notes,
        ReceivedByMemberId = dist.ReceivedByMemberId,
        ReceivedByName = dist.ReceivedByName,
        ReceivedByNationalId = dist.ReceivedByNationalId,
        ReceivedByRelationship = dist.ReceivedByRelationship,
        CharityId = dist.CharityId,
        CharityName = dist.CharityName,
        IsFromCurrentCharity = dist.CharityId == currentCharityId
    }.WithAudit(dist);

    public static Distribution ToEntity(this CreateDistributionDto dto, string charityId, string charityName) => new()
    {
        BeneficiaryId = dto.BeneficiaryId,
        InventoryItemId = dto.InventoryItemId,
        ItemName = dto.ItemName ?? string.Empty,
        Kind = dto.Kind ?? "صنف ملموس",
        Quantity = dto.Quantity,
        Unit = dto.Unit ?? string.Empty,
        Amount = dto.Amount,
        ExpenseCategoryId = dto.ExpenseCategoryId,
        ExpenseCategoryName = dto.ExpenseCategoryName,
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
    public static StaffMemberDto ToDto(this StaffMember s) => new StaffMemberDto()
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
    }.WithAudit(s);

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

    // Geographic Lookups & Addresses
    public static GovernorateDto ToDto(this Governorate g) => new GovernorateDto()
    {
        Id = g.Id,
        Name = g.Name,
        NameEn = g.NameEn,
        DistrictsCount = g.Districts?.Count ?? 0
    }.WithAudit(g);

    public static DistrictDto ToDto(this District d) => new DistrictDto()
    {
        Id = d.Id,
        GovernorateId = d.GovernorateId,
        GovernorateName = d.Governorate?.Name ?? string.Empty,
        Name = d.Name,
        NameEn = d.NameEn,
        VillagesCount = d.Villages?.Count ?? 0
    }.WithAudit(d);

    public static VillageDto ToDto(this Village v) => new VillageDto()
    {
        Id = v.Id,
        DistrictId = v.DistrictId,
        DistrictName = v.District?.Name ?? string.Empty,
        GovernorateId = v.District?.GovernorateId ?? string.Empty,
        GovernorateName = v.District?.Governorate?.Name ?? string.Empty,
        Name = v.Name,
        NameEn = v.NameEn,
        IsActive = v.IsActive,
        AreasCount = v.Areas?.Count ?? 0,
        BeneficiariesCount = v.Beneficiaries?.Count ?? 0,
        CharityId = v.CharityId,
        CharityName = v.CharityName
    }.WithAudit(v);

    public static VillageAreaDto ToDto(this VillageArea a) => new VillageAreaDto()
    {
        Id = a.Id,
        VillageId = a.VillageId,
        VillageName = a.Village?.Name ?? string.Empty,
        DistrictId = a.Village?.DistrictId ?? string.Empty,
        DistrictName = a.Village?.District?.Name ?? string.Empty,
        GovernorateId = a.Village?.District?.GovernorateId ?? string.Empty,
        GovernorateName = a.Village?.District?.Governorate?.Name ?? string.Empty,
        Name = a.Name,
        NameEn = a.NameEn,
        Notes = a.Notes,
        IsActive = a.IsActive,
        BeneficiariesCount = a.Beneficiaries?.Count ?? 0,
        CharityId = a.CharityId,
        CharityName = a.CharityName
    }.WithAudit(a);

    public static AddressRecordDto ToDto(this AddressRecord a) => new AddressRecordDto()
    {
        Id = a.Id,
        GovernorateId = a.GovernorateId,
        DistrictId = a.DistrictId,
        AddressText = a.AddressText,
        UsageCount = a.UsageCount
    }.WithAudit(a);

    // ExpenseCategory
    public static ExpenseCategoryDto ToDto(this ExpenseCategory c) => new ExpenseCategoryDto()
    {
        Id = c.Id,
        Name = c.Name,
        Code = c.Code,
        Description = c.Description,
        IsActive = c.IsActive,
        CharityId = c.CharityId,
        CharityName = c.CharityName
    }.WithAudit(c);
}
