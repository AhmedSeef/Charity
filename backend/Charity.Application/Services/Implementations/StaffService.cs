using Charity.Application.DTOs.Staff;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;

namespace Charity.Application.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly IRepository<StaffMember> _staffRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public StaffService(
        IRepository<StaffMember> staffRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _staffRepo = staffRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<StaffMemberDto>> GetMyStaffAsync()
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var staff = await _staffRepo.FindAsync(s => s.CharityId == charityId);
        return staff.Select(s => s.ToDto()).ToList();
    }

    public async Task<StaffMemberDto?> GetByIdAsync(string id)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var member = await _staffRepo.GetByIdAsync(id);
        if (member == null || member.CharityId != charityId) return null;
        return member.ToDto();
    }

    public async Task<StaffMemberDto> CreateAsync(CreateStaffMemberDto dto)
    {
        var charityId = _currentUserService.CharityId ?? "c1";
        var entity = dto.ToEntity(charityId);

        await _staffRepo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<StaffMemberDto> UpdateAsync(string id, UpdateStaffMemberDto dto)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _staffRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(StaffMember), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك تعديل بيانات موظف لجمعية أخرى!");
        }

        existing.NationalId = dto.NationalId;
        existing.Name = dto.Name;
        existing.Phone = dto.Phone;
        existing.Type = dto.Type;
        existing.Role = dto.Role;
        existing.Salary = dto.Type == "متطوع" ? null : dto.Salary;
        existing.JoinDate = dto.JoinDate;
        existing.Address = dto.Address;
        existing.Tasks = dto.Tasks;
        existing.Status = dto.Status;

        await _staffRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToDto();
    }

    public async Task DeleteAsync(string id)
    {
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var existing = await _staffRepo.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(StaffMember), id);

        if (existing.CharityId != charityId)
        {
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك حذف موظف لجمعية أخرى!");
        }

        await _staffRepo.DeleteAsync(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
