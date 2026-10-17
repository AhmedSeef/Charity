using Charity.Application.DTOs.Lookups;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly IRepository<ExpenseCategory> _categoryRepo;
    private readonly IRepository<Distribution> _distributionRepo;
    private readonly IRepository<Beneficiary> _beneficiaryRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ExpenseCategoryService(
        IRepository<ExpenseCategory> categoryRepo,
        IRepository<Distribution> distributionRepo,
        IRepository<Beneficiary> beneficiaryRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _categoryRepo = categoryRepo;
        _distributionRepo = distributionRepo;
        _beneficiaryRepo = beneficiaryRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(string? month = null)
    {
        var currentCharityId = _currentUserService.CharityId;
        var isAdmin = _currentUserService.Role == "Admin";

        // عرض البنود: بنود النظام العام + بنود جمعية المستخدم (إذا كان مدير جمعية)
        var allCategories = await _categoryRepo.GetAllAsync();
        var categories = (!isAdmin && !string.IsNullOrEmpty(currentCharityId))
            ? allCategories.Where(c => c.CharityId == null || c.CharityId == currentCharityId).ToList()
            : allCategories.ToList();

        // حساب حركات الصرف: إذا كان مدير جمعية، تُحسب المبالغ والحركات الخاصة بجمعيتهم
        var allDistributions = await _distributionRepo.GetAllAsync();
        var distributions = (!isAdmin && !string.IsNullOrEmpty(currentCharityId))
            ? allDistributions.Where(d => d.CharityId == currentCharityId).ToList()
            : allDistributions.ToList();

        // فلترة بالشهر إن وُجد (مثلاً: "2026-08" أو "2026-10")
        if (!string.IsNullOrWhiteSpace(month) && month != "all")
        {
            var trimmedMonth = month.Trim();
            distributions = distributions.Where(d => !string.IsNullOrEmpty(d.Date) && d.Date.StartsWith(trimmedMonth)).ToList();
        }

        return categories.Select(c =>
        {
            var dto = c.ToDto();
            var catDists = distributions.Where(d => d.ExpenseCategoryId == c.Id).ToList();

            dto.TotalDisbursedAmount = catDists.Sum(d => d.Amount ?? 0);
            dto.DisbursementsCount = catDists.Count;
            dto.BeneficiariesCount = catDists.Select(d => d.BeneficiaryId).Distinct().Count();
            return dto;
        }).OrderBy(c => c.Code).ToList();
    }

    public async Task<ExpenseCategoryDto> GetByIdAsync(string id, string? month = null)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(ExpenseCategory), id);

        var currentCharityId = _currentUserService.CharityId;
        var isAdmin = _currentUserService.Role == "Admin";

        var allDistributions = await _distributionRepo.FindAsync(d => d.ExpenseCategoryId == id);
        var catDists = (!isAdmin && !string.IsNullOrEmpty(currentCharityId))
            ? allDistributions.Where(d => d.CharityId == currentCharityId).ToList()
            : allDistributions.ToList();

        if (!string.IsNullOrWhiteSpace(month) && month != "all")
        {
            var trimmedMonth = month.Trim();
            catDists = catDists.Where(d => !string.IsNullOrEmpty(d.Date) && d.Date.StartsWith(trimmedMonth)).ToList();
        }

        var dto = category.ToDto();
        dto.TotalDisbursedAmount = catDists.Sum(d => d.Amount ?? 0);
        dto.DisbursementsCount = catDists.Count;
        dto.BeneficiariesCount = catDists.Select(d => d.BeneficiaryId).Distinct().Count();

        return dto;
    }

    public async Task<IReadOnlyList<ExpenseCategoryDisbursementDetailDto>> GetDisbursementsByCategoryAsync(string categoryId, string? month = null)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId)
            ?? throw new EntityNotFoundException(nameof(ExpenseCategory), categoryId);

        var currentCharityId = _currentUserService.CharityId;
        var isAdmin = _currentUserService.Role == "Admin";

        var distributions = await _distributionRepo.FindAsync(d => d.ExpenseCategoryId == categoryId);
        var distList = (!isAdmin && !string.IsNullOrEmpty(currentCharityId))
            ? distributions.Where(d => d.CharityId == currentCharityId).ToList()
            : distributions.ToList();

        if (!string.IsNullOrWhiteSpace(month) && month != "all")
        {
            var trimmedMonth = month.Trim();
            distList = distList.Where(d => !string.IsNullOrEmpty(d.Date) && d.Date.StartsWith(trimmedMonth)).ToList();
        }

        distList = distList.OrderByDescending(d => d.Date).ToList();

        if (distList.Count == 0)
        {
            return new List<ExpenseCategoryDisbursementDetailDto>();
        }

        var beneficiaryIds = distList.Select(d => d.BeneficiaryId).Distinct().ToList();
        var allBeneficiaries = await _beneficiaryRepo.GetAllAsync();
        var benDict = allBeneficiaries.Where(b => beneficiaryIds.Contains(b.Id)).ToDictionary(b => b.Id);

        return distList.Select(d =>
        {
            benDict.TryGetValue(d.BeneficiaryId, out var ben);

            return new ExpenseCategoryDisbursementDetailDto
            {
                DistributionId = d.Id,
                BeneficiaryId = d.BeneficiaryId,
                BeneficiaryName = ben?.Name ?? d.Beneficiary?.Name ?? string.Empty,
                FamilyCode = ben?.FamilyCode ?? string.Empty,
                FamilyName = ben?.FamilyName ?? string.Empty,
                NationalId = ben?.NationalId ?? string.Empty,
                Phone = ben?.Phone ?? string.Empty,
                GovernorateName = ben?.GovernorateName ?? string.Empty,
                DistrictName = ben?.DistrictName ?? string.Empty,
                Amount = d.Amount,
                Date = d.Date,
                ReceivedByName = d.ReceivedByName ?? ben?.Name,
                ReceivedByRelationship = d.ReceivedByRelationship ?? "صاحب الملف (رب الأسرة)",
                ReceivedByNationalId = d.ReceivedByNationalId ?? ben?.NationalId,
                CharityName = d.CharityName,
                Notes = d.Notes
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<MonthlyExpenseMovementDto>> GetMonthlyMovementsAsync(int? year = null)
    {
        var currentCharityId = _currentUserService.CharityId;
        var isAdmin = _currentUserService.Role == "Admin";

        var allDistributions = await _distributionRepo.GetAllAsync();
        // فقط الحركات النقدية المرتبطة ببند مصروفات
        var distributions = allDistributions.Where(d => d.Kind == "إعانة نقدية" && !string.IsNullOrEmpty(d.ExpenseCategoryId));

        if (!isAdmin && !string.IsNullOrEmpty(currentCharityId))
        {
            distributions = distributions.Where(d => d.CharityId == currentCharityId);
        }

        if (year.HasValue)
        {
            var yearStr = year.Value.ToString();
            distributions = distributions.Where(d => !string.IsNullOrEmpty(d.Date) && d.Date.StartsWith(yearStr));
        }

        var list = distributions.ToList();

        // التجميع حسب الشهر والسنة (YYYY-MM)
        var grouped = list
            .Where(d => !string.IsNullOrWhiteSpace(d.Date) && d.Date.Length >= 7)
            .GroupBy(d => d.Date.Substring(0, 7))
            .OrderByDescending(g => g.Key)
            .ToList();

        var result = new List<MonthlyExpenseMovementDto>();

        foreach (var g in grouped)
        {
            var monthYear = g.Key; // e.g. "2026-10"
            var parts = monthYear.Split('-');
            int y = 0, m = 0;
            if (parts.Length == 2)
            {
                int.TryParse(parts[0], out y);
                int.TryParse(parts[1], out m);
            }

            var monthLabel = m > 0 ? $"شهر {m}/{y}" : monthYear;

            var topCats = g
                .GroupBy(d => new { d.ExpenseCategoryId, d.ExpenseCategoryName })
                .Select(cg => new CategoryMonthlyShareDto
                {
                    CategoryId = cg.Key.ExpenseCategoryId ?? string.Empty,
                    CategoryName = cg.Key.ExpenseCategoryName ?? string.Empty,
                    Amount = cg.Sum(d => d.Amount ?? 0),
                    DisbursementsCount = cg.Count()
                })
                .OrderByDescending(cg => cg.Amount)
                .ToList();

            result.Add(new MonthlyExpenseMovementDto
            {
                MonthYear = monthYear,
                MonthLabel = monthLabel,
                Year = y,
                Month = m,
                TotalAmount = g.Sum(d => d.Amount ?? 0),
                DisbursementsCount = g.Count(),
                BeneficiariesCount = g.Select(d => d.BeneficiaryId).Distinct().Count(),
                TopCategories = topCats
            });
        }

        return result;
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationDomainException("اسم نوع أو بند المصروفات مطلوب!");
        }

        var charityId = _currentUserService.CharityId;
        var charityName = _currentUserService.CharityName ?? "النظام العام";

        var trimmedName = dto.Name.Trim();
        var existing = await _categoryRepo.FirstOrDefaultAsync(c =>
            c.Name.ToLower() == trimmedName.ToLower() && (c.CharityId == null || c.CharityId == charityId));

        if (existing != null)
        {
            throw new ValidationDomainException($"بند المصروفات باسم ({trimmedName}) مسجل مسبقاً!");
        }

        var code = dto.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            var totalCount = await _categoryRepo.CountAsync();
            code = $"EXP-{(totalCount + 1):D3}";
        }

        var entity = new ExpenseCategory
        {
            Name = trimmedName,
            Code = code,
            Description = dto.Description?.Trim() ?? string.Empty,
            IsActive = true,
            CharityId = charityId,
            CharityName = charityName
        };

        await _categoryRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(string id, UpdateExpenseCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationDomainException("اسم نوع أو بند المصروفات مطلوب!");
        }

        var entity = await _categoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(ExpenseCategory), id);

        var currentCharityId = _currentUserService.CharityId;
        var isAdmin = _currentUserService.Role == "Admin";

        if (!isAdmin && entity.CharityId != null && entity.CharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException("لا يمكنك تعديل بند مصروفات يتبع جمعية أخرى.");
        }

        var trimmedName = dto.Name.Trim();
        var duplicate = await _categoryRepo.FirstOrDefaultAsync(c =>
            c.Id != id && c.Name.ToLower() == trimmedName.ToLower() && (c.CharityId == null || c.CharityId == currentCharityId));

        if (duplicate != null)
        {
            throw new ValidationDomainException($"يوجد بند مصروفات آخر مسجل بنفس الاسم ({trimmedName})!");
        }

        entity.Name = trimmedName;
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            entity.Code = dto.Code.Trim();
        }
        entity.Description = dto.Description?.Trim() ?? string.Empty;
        entity.IsActive = dto.IsActive;

        await _categoryRepo.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _categoryRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(ExpenseCategory), id);

        var currentCharityId = _currentUserService.CharityId;
        var isAdmin = _currentUserService.Role == "Admin";

        if (!isAdmin && entity.CharityId == null)
        {
            throw new ValidationDomainException("⛔ محظور الحذف: لا يمكن لمدير الجمعية حذف بنود المصروفات العامة للنظام، ويُسمح فقط بحذف البنود المضافة من قبل جمعيتكم.");
        }

        if (!isAdmin && entity.CharityId != currentCharityId)
        {
            throw new UnauthorizedDomainException("لا يمكنك حذف بند مصروفات يتبع جمعية أخرى.");
        }

        // قاعدة منع الحذف الصارمة: التحقق من عدم وجود أي حركة صرف تمت على هذا البند
        var allDistributions = await _distributionRepo.GetAllIncludingDeletedAsync();
        var linkedDistributions = allDistributions.Where(d => d.ExpenseCategoryId == id).ToList();

        if (linkedDistributions.Count > 0)
        {
            var totalAmount = linkedDistributions.Sum(d => d.Amount ?? 0);
            throw new ValidationDomainException(
                $"⛔ محظور الحذف: لا يمكن حذف بند المصروفات ({entity.Name}) لوجود ({linkedDistributions.Count}) حركة صرف سابقة مسجلة عليه بإجمالي مبلغ ({totalAmount:N0} ج.م)! " +
                "لحماية وتدقيق السجلات المالية والحسابات، يُمنع منعاً باتاً حذف أي بند تمت عليه حركات صرف.");
        }

        await _categoryRepo.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
