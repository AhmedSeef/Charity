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
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SuspensionReasonService(
        IRepository<SuspensionReason> repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SuspensionReasonDto>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return list.Select(r => r.ToDto()).ToList();
    }

    public async Task<SuspensionReasonDto> CreateAsync(CreateSuspensionReasonDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationDomainException("عنوان أو سبب الإيقاف مطلوب!");
        }

        var charityId = _currentUserService.CharityId ?? "c1";
        var charityName = _currentUserService.CharityName ?? "الجمعية العامة";

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
        var existing = await _repository.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(SuspensionReason), id);

        await _repository.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
