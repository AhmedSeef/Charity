using Charity.Application.DTOs.SeparationReasons;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class SeparationReasonService : ISeparationReasonService
{
    private readonly IRepository<SeparationReason> _repository;
    private readonly IRepository<BeneficiaryFamilyMember> _familyMemberRepository;
    private readonly IRepository<Beneficiary> _beneficiaryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SeparationReasonService(
        IRepository<SeparationReason> repository,
        IRepository<BeneficiaryFamilyMember> familyMemberRepository,
        IRepository<Beneficiary> beneficiaryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _familyMemberRepository = familyMemberRepository;
        _beneficiaryRepository = beneficiaryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SeparationReasonDto>> GetAllAsync()
    {
        var list = (await _repository.GetAllAsync()).Where(r => TenantAccess.CanRead(r.CharityId, _currentUserService));
        var allBeneficiaries = (await _beneficiaryRepository.GetAllIncludingDeletedAsync())
            .Where(b => TenantAccess.CanRead(b.RegisteredByCharityId, _currentUserService)).ToList();
        var ownedIds = allBeneficiaries.Select(b => b.Id).ToHashSet();
        var allMembers = (await _familyMemberRepository.GetAllIncludingDeletedAsync()).Where(m => ownedIds.Contains(m.BeneficiaryId)).ToList();

        return list.Select(r =>
        {
            var dto = r.ToDto();
            
            // حساب عدد العمليات التي استخدمت هذا السبب
            var linkedMembersCount = allMembers.Count(m =>
                m.SeparationReasonId == r.Id || (!string.IsNullOrEmpty(m.SeparationReason) && m.SeparationReason == r.Title));
            
            var linkedBeneficiariesCount = allBeneficiaries.Count(b =>
                !string.IsNullOrEmpty(b.ResearchNotes) && b.ResearchNotes.Contains(r.Title));

            dto.LinkedOperationsCount = Math.Max(linkedMembersCount, linkedBeneficiariesCount);
            // شرط الحذف الحاسم: لا يمكن الحذف إذا تم استخدامه في أي عملية انفصال
            dto.CanDelete = dto.LinkedOperationsCount == 0;
            return dto;
        }).ToList();
    }

    public async Task<SeparationReasonDto> CreateAsync(CreateSeparationReasonDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationDomainException("عنوان أو مسمى سبب الانفصال مطلوب!");
        }

        var charityId = _currentUserService.CharityId;
        if (string.IsNullOrWhiteSpace(charityId) && _currentUserService.Role != "Admin")
            TenantAccess.RequireCharity(_currentUserService);
        var charityName = _currentUserService.CharityName ?? "النظام العام";

        var entity = new SeparationReason
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
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(SeparationReason), id);
        TenantAccess.DemandOwner(existing.CharityId, _currentUserService);

        // الشرط الإلزامي: التحقق من عدم وجود أي عملية انفصال سابقة تمت بهذا السبب
        var allBeneficiaries = await _beneficiaryRepository.GetAllIncludingDeletedAsync();
        var ownedIds = allBeneficiaries.Where(b => b.RegisteredByCharityId == existing.CharityId).Select(b => b.Id).ToHashSet();
        var allMembers = await _familyMemberRepository.GetAllIncludingDeletedAsync();
        var linkedMembers = allMembers
            .Where(m => m.SeparationReasonId == existing.Id || (ownedIds.Contains(m.BeneficiaryId) && !string.IsNullOrEmpty(m.SeparationReason) && m.SeparationReason == existing.Title))
            .ToList();

        var linkedBeneficiaries = allBeneficiaries
            .Where(b => b.RegisteredByCharityId == existing.CharityId && !string.IsNullOrEmpty(b.ResearchNotes) && b.ResearchNotes.Contains(existing.Title))
            .ToList();

        var totalOperations = Math.Max(linkedMembers.Count, linkedBeneficiaries.Count);

        if (totalOperations > 0)
        {
            throw new ValidationDomainException(
                $"⛔ محظور الحذف: لا يمكن حذف سبب الانفصال ({existing.Title}) لأنه تم استخدامه في ({totalOperations}) عملية انفصال سابقة! لحماية وتدقيق السجلات، يُسمح فقط بحذف الأسباب التي لم تُنفذ عليها أي عملية.");
        }

        await _repository.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
