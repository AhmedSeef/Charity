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
        var list = await _repository.GetAllAsync();
        var allMembers = await _familyMemberRepository.GetAllIncludingDeletedAsync();
        var allBeneficiaries = await _beneficiaryRepository.GetAllIncludingDeletedAsync();

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

        // الشرط الإلزامي: التحقق من عدم وجود أي عملية انفصال سابقة تمت بهذا السبب
        var allMembers = await _familyMemberRepository.GetAllIncludingDeletedAsync();
        var linkedMembers = allMembers
            .Where(m => m.SeparationReasonId == existing.Id || (!string.IsNullOrEmpty(m.SeparationReason) && m.SeparationReason == existing.Title))
            .ToList();

        var allBeneficiaries = await _beneficiaryRepository.GetAllIncludingDeletedAsync();
        var linkedBeneficiaries = allBeneficiaries
            .Where(b => !string.IsNullOrEmpty(b.ResearchNotes) && b.ResearchNotes.Contains(existing.Title))
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
