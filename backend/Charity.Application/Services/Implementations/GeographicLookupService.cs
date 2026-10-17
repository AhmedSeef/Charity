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
    private readonly IRepository<AddressRecord> _addressRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GeographicLookupService(
        IRepository<Governorate> govRepo,
        IRepository<District> districtRepo,
        IRepository<AddressRecord> addressRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _govRepo = govRepo;
        _districtRepo = districtRepo;
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
                return dto;
            })
            .ToList();
    }

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
        var candidates = allAddresses.AsEnumerable();
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
        if (string.IsNullOrWhiteSpace(dto.AddressText))
            throw new ValidationDomainException("نص العنوان مطلوب.");

        var text = dto.AddressText.Trim();
        var normalized = AddressNormalizationService.Normalize(text);

        // البحث عما إذا كان هذا العنوان مسجلاً مسبقاً في نفس المركز
        var existing = await _addressRepo.FirstOrDefaultAsync(a =>
            a.DistrictId == dto.DistrictId &&
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
