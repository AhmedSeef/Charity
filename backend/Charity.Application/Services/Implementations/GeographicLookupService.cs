using Charity.Application.DTOs.Lookups;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class GeographicLookupService : IGeographicLookupService
{
    private readonly IRepository<Governorate> _govRepo;
    private readonly IRepository<District> _districtRepo;
    private readonly IRepository<Village> _villageRepo;
    private readonly IRepository<VillageArea> _villageAreaRepo;
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly IRepository<AddressRecord> _addressRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GeographicLookupService(
        IRepository<Governorate> govRepo,
        IRepository<District> districtRepo,
        IRepository<Village> villageRepo,
        IRepository<VillageArea> villageAreaRepo,
        IRepository<Beneficiary> beneficiaryRepo,
        IRepository<AddressRecord> addressRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _govRepo = govRepo;
        _districtRepo = districtRepo;
        _villageRepo = villageRepo;
        _villageAreaRepo = villageAreaRepo;
        _beneficiaryRepo = beneficiaryRepo;
        _addressRepo = addressRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<GovernorateDto>> GetGovernoratesAsync()
    {
        var govs = await _govRepo.GetAllAsync();
        var allDistricts = await _districtRepo.GetAllAsync();

        return govs
            .OrderBy(g => g.Name.Contains("كفر الشيخ") ? 0 : 1)
            .ThenBy(g => g.Name)
            .Select(g =>
            {
                var dto = g.ToDto();
                dto.DistrictsCount = allDistricts.Count(d => d.GovernorateId == g.Id);
                return dto;
            })
            .ToList();
    }

    public async Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(string? governorateId = null)
    {
        var districts = string.IsNullOrWhiteSpace(governorateId)
            ? await _districtRepo.GetAllAsync()
            : await _districtRepo.FindAsync(d => d.GovernorateId == governorateId);

        var allGovs = (await _govRepo.GetAllAsync()).ToDictionary(g => g.Id, g => g.Name);
        var allVillages = (await _villageRepo.GetAllAsync()).Where(v => TenantAccess.CanRead(v.CharityId, _currentUserService)).ToList();

        return districts
            .OrderBy(d => d.Name.Contains("مطوبس") ? 0 : 1)
            .ThenBy(d => d.Name)
            .Select(d =>
            {
                var dto = d.ToDto();
                if (allGovs.TryGetValue(d.GovernorateId, out var gName))
                {
                    dto.GovernorateName = gName;
                }
                dto.VillagesCount = allVillages.Count(v => v.DistrictId == d.Id);
                return dto;
            })
            .ToList();
    }

    // ==========================================
    // Villages (القرى التابعة للمراكز)
    // ==========================================

    public async Task<IReadOnlyList<VillageDto>> GetVillagesAsync(string? districtId = null)
    {
        var villages = string.IsNullOrWhiteSpace(districtId)
            ? await _villageRepo.GetAllAsync()
            : await _villageRepo.FindAsync(v => v.DistrictId == districtId);

        var allDistricts = (await _districtRepo.GetAllAsync()).ToDictionary(d => d.Id);
        var allGovs = (await _govRepo.GetAllAsync()).ToDictionary(g => g.Id, g => g.Name);
        var allAreas = (await _villageAreaRepo.GetAllAsync()).Where(a => TenantAccess.CanRead(a.CharityId, _currentUserService)).ToList();
        var allBeneficiaries = (await _beneficiaryRepo.GetAllAsync()).Where(b => TenantAccess.CanRead(b.RegisteredByCharityId, _currentUserService)).ToList();

        return villages.Where(v => TenantAccess.CanRead(v.CharityId, _currentUserService))
            .OrderBy(v => v.Name.Contains("منية المرشد") ? 0 : 1)
            .ThenBy(v => v.Name)
            .Select(v =>
            {
                var dto = v.ToDto();
                if (allDistricts.TryGetValue(v.DistrictId, out var dist))
                {
                    dto.DistrictName = dist.Name;
                    dto.GovernorateId = dist.GovernorateId;
                    if (allGovs.TryGetValue(dist.GovernorateId, out var govName))
                    {
                        dto.GovernorateName = govName;
                    }
                }
                dto.AreasCount = allAreas.Count(a => a.VillageId == v.Id);
                dto.BeneficiariesCount = allBeneficiaries.Count(b => b.VillageId == v.Id);
                return dto;
            })
            .ToList();
    }

    public async Task<VillageDto> GetVillageByIdAsync(string id)
    {
        var village = await _villageRepo.GetByIdAsync(id);
        if (village == null || !TenantAccess.CanRead(village.CharityId, _currentUserService))
            throw new EntityNotFoundException(nameof(Village), id);

        var dto = village.ToDto();
        var district = await _districtRepo.GetByIdAsync(village.DistrictId);
        if (district != null)
        {
            dto.DistrictName = district.Name;
            dto.GovernorateId = district.GovernorateId;
            var gov = await _govRepo.GetByIdAsync(district.GovernorateId);
            if (gov != null)
            {
                dto.GovernorateName = gov.Name;
            }
        }

        dto.AreasCount = await _villageAreaRepo.CountAsync(a => a.VillageId == village.Id && a.CharityId == village.CharityId);
        dto.BeneficiariesCount = await _beneficiaryRepo.CountAsync(b => b.VillageId == village.Id && b.RegisteredByCharityId == village.CharityId);
        return dto;
    }

    public async Task<VillageDto> CreateVillageAsync(CreateVillageDto dto)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.CharityId) && _currentUserService.Role != "Admin")
            TenantAccess.RequireCharity(_currentUserService);
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationDomainException("اسم القرية مطلوب.");

        if (string.IsNullOrWhiteSpace(dto.DistrictId))
            throw new ValidationDomainException("يجب اختيار المركز أو الحي التابع له القرية.");

        var district = await _districtRepo.GetByIdAsync(dto.DistrictId);
        if (district == null)
            throw new EntityNotFoundException(nameof(District), dto.DistrictId);

        var nameTrimmed = dto.Name.Trim();
        var duplicate = await _villageRepo.FirstOrDefaultAsync(v =>
            v.DistrictId == dto.DistrictId && v.CharityId == _currentUserService.CharityId && v.Name.ToLower() == nameTrimmed.ToLower());

        if (duplicate != null)
            throw new ValidationDomainException($"القرية '{nameTrimmed}' مسجلة بالفعل في هذا المركز.");

        var village = new Village
        {
            Id = Guid.NewGuid().ToString(),
            DistrictId = dto.DistrictId,
            Name = nameTrimmed,
            NameEn = dto.NameEn?.Trim() ?? string.Empty,
            IsActive = true,
            CharityId = _currentUserService.CharityId,
            CharityName = _currentUserService.CharityName
        };

        await _villageRepo.AddAsync(village);
        await _unitOfWork.SaveChangesAsync();

        return await GetVillageByIdAsync(village.Id);
    }

    public async Task<VillageDto> UpdateVillageAsync(string id, UpdateVillageDto dto)
    {
        var village = await _villageRepo.GetByIdAsync(id);
        if (village == null)
            throw new EntityNotFoundException(nameof(Village), id);
        TenantAccess.DemandOwner(village.CharityId, _currentUserService);

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationDomainException("اسم القرية مطلوب.");

        var nameTrimmed = dto.Name.Trim();
        var duplicate = await _villageRepo.FirstOrDefaultAsync(v =>
            v.DistrictId == village.DistrictId &&
            v.CharityId == _currentUserService.CharityId &&
            v.Id != id &&
            v.Name.ToLower() == nameTrimmed.ToLower());

        if (duplicate != null)
            throw new ValidationDomainException($"توجد قرية أخرى مسجلة بنفس الاسم '{nameTrimmed}' في هذا المركز.");

        village.Name = nameTrimmed;
        village.NameEn = dto.NameEn?.Trim() ?? string.Empty;
        village.IsActive = dto.IsActive;

        await _villageRepo.UpdateAsync(village);
        await _unitOfWork.SaveChangesAsync();

        return await GetVillageByIdAsync(village.Id);
    }

    public async Task DeleteVillageAsync(string id)
    {
        var village = await _villageRepo.GetByIdAsync(id);
        if (village == null)
            throw new EntityNotFoundException(nameof(Village), id);
        TenantAccess.DemandOwner(village.CharityId, _currentUserService);

        var areasCount = await _villageAreaRepo.CountAsync(a => a.VillageId == id);
        if (areasCount > 0)
            throw new ValidationDomainException($"لا يمكن حذف القرية '{village.Name}' لوجود {areasCount} منطقة/حي تابع لها. يرجى حذف أو نقل المناطق التابعة أولاً.");

        var benCount = await _beneficiaryRepo.CountAsync(b => b.VillageId == id);
        if (benCount > 0)
            throw new ValidationDomainException($"لا يمكن حذف القرية '{village.Name}' لوجود {benCount} مستفيد مسجل عليها.");

        await _villageRepo.DeleteAsync(village);
        await _unitOfWork.SaveChangesAsync();
    }

    // ==========================================
    // Village Areas (المناطق والأحياء التابعة للقرى)
    // ==========================================

    public async Task<IReadOnlyList<VillageAreaDto>> GetVillageAreasAsync(string? villageId = null)
    {
        var areas = string.IsNullOrWhiteSpace(villageId)
            ? await _villageAreaRepo.GetAllAsync()
            : await _villageAreaRepo.FindAsync(a => a.VillageId == villageId);

        var allVillages = (await _villageRepo.GetAllAsync()).ToDictionary(v => v.Id);
        var allDistricts = (await _districtRepo.GetAllAsync()).ToDictionary(d => d.Id);
        var allGovs = (await _govRepo.GetAllAsync()).ToDictionary(g => g.Id, g => g.Name);
        var allBeneficiaries = (await _beneficiaryRepo.GetAllAsync()).Where(b => TenantAccess.CanRead(b.RegisteredByCharityId, _currentUserService)).ToList();

        // ترتيب خاص لمناطق منية المرشد الشائعة
        var priorityOrder = new Dictionary<string, int>
        {
            { "المنشر", 1 },
            { "السوق", 2 },
            { "الجسر", 3 },
            { "المضرب", 4 },
            { "الترب", 5 }
        };

        return areas.Where(a => TenantAccess.CanRead(a.CharityId, _currentUserService))
            .OrderBy(a => priorityOrder.TryGetValue(a.Name.Trim(), out var p) ? p : 99)
            .ThenBy(a => a.Name)
            .Select(a =>
            {
                var dto = a.ToDto();
                if (allVillages.TryGetValue(a.VillageId, out var vil))
                {
                    dto.VillageName = vil.Name;
                    dto.DistrictId = vil.DistrictId;
                    if (allDistricts.TryGetValue(vil.DistrictId, out var dist))
                    {
                        dto.DistrictName = dist.Name;
                        dto.GovernorateId = dist.GovernorateId;
                        if (allGovs.TryGetValue(dist.GovernorateId, out var govName))
                        {
                            dto.GovernorateName = govName;
                        }
                    }
                }
                dto.BeneficiariesCount = allBeneficiaries.Count(b => b.VillageAreaId == a.Id);
                return dto;
            })
            .ToList();
    }

    public async Task<VillageAreaDto> GetVillageAreaByIdAsync(string id)
    {
        var area = await _villageAreaRepo.GetByIdAsync(id);
        if (area == null || !TenantAccess.CanRead(area.CharityId, _currentUserService))
            throw new EntityNotFoundException(nameof(VillageArea), id);

        var dto = area.ToDto();
        var village = await _villageRepo.GetByIdAsync(area.VillageId);
        if (village != null)
        {
            dto.VillageName = village.Name;
            dto.DistrictId = village.DistrictId;
            var district = await _districtRepo.GetByIdAsync(village.DistrictId);
            if (district != null)
            {
                dto.DistrictName = district.Name;
                dto.GovernorateId = district.GovernorateId;
                var gov = await _govRepo.GetByIdAsync(district.GovernorateId);
                if (gov != null)
                {
                    dto.GovernorateName = gov.Name;
                }
            }
        }

        dto.BeneficiariesCount = await _beneficiaryRepo.CountAsync(b => b.VillageAreaId == area.Id && b.RegisteredByCharityId == area.CharityId);
        return dto;
    }

    public async Task<VillageAreaDto> CreateVillageAreaAsync(CreateVillageAreaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationDomainException("اسم المنطقة أو الحي مطلوب.");

        if (string.IsNullOrWhiteSpace(dto.VillageId))
            throw new ValidationDomainException("يجب اختيار القرية التابعة لها المنطقة.");

        var village = await _villageRepo.GetByIdAsync(dto.VillageId);
        if (village == null)
            throw new EntityNotFoundException(nameof(Village), dto.VillageId);
        TenantAccess.DemandOwner(village.CharityId, _currentUserService);

        var nameTrimmed = dto.Name.Trim();
        var duplicate = await _villageAreaRepo.FirstOrDefaultAsync(a =>
            a.VillageId == dto.VillageId && a.CharityId == _currentUserService.CharityId && a.Name.ToLower() == nameTrimmed.ToLower());

        if (duplicate != null)
            throw new ValidationDomainException($"المنطقة '{nameTrimmed}' مسجلة بالفعل في هذه القرية.");

        var area = new VillageArea
        {
            Id = Guid.NewGuid().ToString(),
            VillageId = dto.VillageId,
            Name = nameTrimmed,
            NameEn = dto.NameEn?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim() ?? string.Empty,
            IsActive = true,
            CharityId = _currentUserService.CharityId,
            CharityName = _currentUserService.CharityName
        };

        await _villageAreaRepo.AddAsync(area);
        await _unitOfWork.SaveChangesAsync();

        return await GetVillageAreaByIdAsync(area.Id);
    }

    public async Task<VillageAreaDto> UpdateVillageAreaAsync(string id, UpdateVillageAreaDto dto)
    {
        var area = await _villageAreaRepo.GetByIdAsync(id);
        if (area == null)
            throw new EntityNotFoundException(nameof(VillageArea), id);
        TenantAccess.DemandOwner(area.CharityId, _currentUserService);

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationDomainException("اسم المنطقة مطلوب.");

        var nameTrimmed = dto.Name.Trim();
        var duplicate = await _villageAreaRepo.FirstOrDefaultAsync(a =>
            a.VillageId == area.VillageId &&
            a.CharityId == _currentUserService.CharityId &&
            a.Id != id &&
            a.Name.ToLower() == nameTrimmed.ToLower());

        if (duplicate != null)
            throw new ValidationDomainException($"توجد منطقة أخرى بنفس الاسم '{nameTrimmed}' مسجلة في هذه القرية.");

        area.Name = nameTrimmed;
        area.NameEn = dto.NameEn?.Trim() ?? string.Empty;
        area.Notes = dto.Notes?.Trim() ?? string.Empty;
        area.IsActive = dto.IsActive;

        await _villageAreaRepo.UpdateAsync(area);
        await _unitOfWork.SaveChangesAsync();

        return await GetVillageAreaByIdAsync(area.Id);
    }

    public async Task DeleteVillageAreaAsync(string id)
    {
        var area = await _villageAreaRepo.GetByIdAsync(id);
        if (area == null)
            throw new EntityNotFoundException(nameof(VillageArea), id);
        TenantAccess.DemandOwner(area.CharityId, _currentUserService);

        var benCount = await _beneficiaryRepo.CountAsync(b => b.VillageAreaId == id);
        if (benCount > 0)
            throw new ValidationDomainException($"لا يمكن حذف المنطقة '{area.Name}' لوجود {benCount} مستفيد مسجل عليها.");

        await _villageAreaRepo.DeleteAsync(area);
        await _unitOfWork.SaveChangesAsync();
    }

    // ==========================================
    // Address Directory & Autocomplete
    // ==========================================

    public async Task<IReadOnlyList<AddressAutocompleteItemDto>> AutocompleteAddressesAsync(
        string? query,
        string? governorateId = null,
        string? districtId = null,
        int limit = 10)
    {
        var allAddresses = await _addressRepo.GetAllAsync();
        var allGovs = (await _govRepo.GetAllAsync()).ToDictionary(g => g.Id, g => g.Name);
        var allDistricts = (await _districtRepo.GetAllAsync()).ToDictionary(d => d.Id, d => d.Name);

        var queryNormalized = AddressNormalizationService.Normalize(query);

        // تصفية حسب المركز أو المحافظة إن وجدت
        var candidates = allAddresses.Where(a => TenantAccess.CanRead(a.CharityId, _currentUserService));
        if (!string.IsNullOrWhiteSpace(districtId))
        {
            candidates = candidates.Where(a => a.DistrictId == districtId);
        }
        else if (!string.IsNullOrWhiteSpace(governorateId))
        {
            candidates = candidates.Where(a => a.GovernorateId == governorateId);
        }

        // إذا كان البحث فارغاً، نعرض الأماكن الأكثر استخداماً في المركز أو المحافظة
        if (string.IsNullOrWhiteSpace(queryNormalized))
        {
            return candidates
                .OrderByDescending(a => a.UsageCount)
                .Take(limit)
                .Select(a => new AddressAutocompleteItemDto
                {
                    Id = a.Id,
                    AddressText = a.AddressText,
                    DistrictId = a.DistrictId,
                    DistrictName = allDistricts.TryGetValue(a.DistrictId, out var dn) ? dn : "",
                    GovernorateId = a.GovernorateId,
                    GovernorateName = allGovs.TryGetValue(a.GovernorateId, out var gn) ? gn : "",
                    MatchScore = 100,
                    UsageCount = a.UsageCount
                })
                .ToList();
        }

        // حساب الأوزان الذكية والنورماليزيشن
        var scoredList = new List<AddressAutocompleteItemDto>();

        foreach (var addr in candidates)
        {
            var normalizedText = string.IsNullOrWhiteSpace(addr.NormalizedText)
                ? AddressNormalizationService.Normalize(addr.AddressText)
                : addr.NormalizedText;

            int score = AddressNormalizationService.CalculateMatchScore(
                addr.AddressText,
                normalizedText,
                queryNormalized,
                addr.UsageCount);

            if (score >= 35) // حد أدنى للتشابه
            {
                scoredList.Add(new AddressAutocompleteItemDto
                {
                    Id = addr.Id,
                    AddressText = addr.AddressText,
                    DistrictId = addr.DistrictId,
                    DistrictName = allDistricts.TryGetValue(addr.DistrictId, out var dn) ? dn : "",
                    GovernorateId = addr.GovernorateId,
                    GovernorateName = allGovs.TryGetValue(addr.GovernorateId, out var gn) ? gn : "",
                    MatchScore = score,
                    UsageCount = addr.UsageCount
                });
            }
        }

        return scoredList
            .OrderByDescending(x => x.MatchScore)
            .ThenByDescending(x => x.UsageCount)
            .Take(limit)
            .ToList();
    }

    public async Task<AddressRecordDto> RecordAddressUsageAsync(CreateAddressRecordDto dto)
    {
        if (string.IsNullOrWhiteSpace(_currentUserService.CharityId) && _currentUserService.Role != "Admin")
            TenantAccess.RequireCharity(_currentUserService);
        if (string.IsNullOrWhiteSpace(dto.AddressText))
            throw new ValidationDomainException("نص العنوان مطلوب.");

        var text = dto.AddressText.Trim();
        var normalized = AddressNormalizationService.Normalize(text);

        // البحث عما إذا كان هذا العنوان مسجلاً مسبقاً في نفس المركز
        var existing = await _addressRepo.FirstOrDefaultAsync(a =>
            a.DistrictId == dto.DistrictId && a.CharityId == _currentUserService.CharityId &&
            (a.NormalizedText == normalized || a.AddressText == text));

        if (existing != null)
        {
            existing.UsageCount++;
            await _addressRepo.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return existing.ToDto();
        }

        var newRecord = new AddressRecord
        {
            GovernorateId = dto.GovernorateId,
            DistrictId = dto.DistrictId,
            AddressText = text,
            NormalizedText = normalized,
            UsageCount = 1,
            CharityId = _currentUserService.CharityId,
            CharityName = _currentUserService.CharityName
        };

        await _addressRepo.AddAsync(newRecord);
        await _unitOfWork.SaveChangesAsync();
        return newRecord.ToDto();
    }

    public async Task<AddressRecordDto> CreateCustomAddressAsync(CreateAddressRecordDto dto)
    {
        return await RecordAddressUsageAsync(dto);
    }
}
