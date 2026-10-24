using Charity.Application.DTOs.SuspensionReasons;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class SuspensionReasonService : ISuspensionReasonService
{
    private readonly IRepository<SuspensionReason> _repository;
    private readonly IRepository<Beneficiary> _beneficiaryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SuspensionReasonService(
        IRepository<SuspensionReason> repository,
        IRepository<Beneficiary> beneficiaryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _beneficiaryRepository = beneficiaryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SuspensionReasonDto>> GetAllAsync()
    {
        LookupAccess.DemandManager(_currentUserService);
        var list = (await _repository.GetAllAsync()).Where(r => TenantAccess.CanRead(r.CharityId, _currentUserService));
        var allBeneficiaries = await _beneficiaryRepository.GetAllAsync();
        var suspendedCases = allBeneficiaries.Where(b => b.IsSuspended && TenantAccess.CanRead(b.RegisteredByCharityId, _currentUserService)).ToList();

        return list.Select(r =>
        {
            var dto = r.ToDto();
            dto.LinkedSuspendedBeneficiariesCount = suspendedCases.Count(b =>
                b.SuspensionReasonId == r.Id || b.SuspensionReasonTitle == r.Title);
            dto.CanDelete = dto.LinkedSuspendedBeneficiariesCount == 0;
            return dto;
        }).ToList();
    }

    public async Task<SuspensionReasonDto> CreateAsync(CreateSuspensionReasonDto dto)
    {
        LookupAccess.DemandManager(_currentUserService);
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationDomainException("عنوان أو سبب الإيقاف مطلوب!");
        }

        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId) && _currentUserService.Role != "Admin")
            TenantAccess.RequireCharity(_currentUserService);
        var charityName = _currentUserService.CharityName ?? "النظام العام";

        var entity = new SuspensionReason
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            IsActive = true,
            CharityId = charityId,
            CharityName = charityName
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task DeleteAsync(string id)
    {
        LookupAccess.DemandManager(_currentUserService);
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(SuspensionReason), id);
        TenantAccess.DemandOwner(existing.CharityId, _currentUserService);

        // التحقق الحاسم: منع حذف أي سبب إيقاف مرتبط بحالات موقوفة حالياً
        var allBeneficiaries = await _beneficiaryRepository.GetAllAsync();
        var linkedSuspendedCases = allBeneficiaries
            .Where(b => b.IsSuspended && (b.SuspensionReasonId == existing.Id ||
                (b.RegisteredByCharityId == existing.CharityId && b.SuspensionReasonTitle == existing.Title)))
            .ToList();

        if (linkedSuspendedCases.Count > 0)
        {
            var count = linkedSuspendedCases.Count;
            var sampleNames = string.Join("، ", linkedSuspendedCases.Take(3).Select(b => b.Name));
            var extra = count > 3 ? $" وغيرها ({count} حالة إجمالاً)" : "";
            throw new ValidationDomainException(
                $"⛔ لا يمكن حذف سبب الإيقاف ({existing.Title}) لوجود ({count}) حالة موقوفة مرتبطة بهذا السبب حالياً: [{sampleNames}{extra}]! يجب تعديل سبب إيقاف تلك الحالات أو إعادة تفعيلها أولاً قبل السماح بالحذف.");
        }

        await _repository.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
