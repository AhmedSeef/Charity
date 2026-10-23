using Charity.Application.DTOs.Staff;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;
using Charity.Domain.Validation;
using System.Text.RegularExpressions;

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
        var charityId = TenantAccess.RequireCharity(_currentUserService);
        ValidateStaff(dto.Name, dto.NationalId, dto.Phone, dto.Type, dto.Role, dto.Salary, dto.JoinDate,
            out var name, out var nationalId, out var phone, out var joinDate);
        var entity = dto.ToEntity(charityId);
        entity.Name = name;
        entity.NationalId = nationalId;
        entity.Phone = phone;
        entity.JoinDate = joinDate;
        entity.Role = dto.Role.Trim();

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

        ValidateStaff(dto.Name, dto.NationalId, dto.Phone, dto.Type, dto.Role, dto.Salary, dto.JoinDate,
            out var name, out var nationalId, out var phone, out var joinDate);
        existing.NationalId = nationalId;
        existing.Name = name;
        existing.Phone = phone;
        existing.Type = dto.Type;
        existing.Role = dto.Role.Trim();
        existing.Salary = dto.Type == "متطوع" ? null : dto.Salary;
        existing.JoinDate = joinDate;
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

    private static void ValidateStaff(string nameValue, string nationalIdValue, string phoneValue,
        string type, string role, decimal? salary, string joinDateValue,
        out string name, out string nationalId, out string phone, out string joinDate)
    {
        name = nameValue?.Trim() ?? string.Empty;
        if (name.Length == 0) throw new ValidationDomainException("اسم عضو فريق العمل مطلوب.");
        nationalId = EgyptianNationalId.Validate(nationalIdValue);
        phone = phoneValue?.Trim() ?? string.Empty;
        if (!Regex.IsMatch(phone, @"^01[0125][0-9]{8}$"))
            throw new ValidationDomainException("رقم هاتف عضو فريق العمل يجب أن يكون رقم محمول مصريًا صحيحًا من 11 رقمًا.");
        if (type is not ("متطوع" or "موظف براتب"))
            throw new ValidationDomainException("نوع عضو فريق العمل غير صحيح.");
        if (string.IsNullOrWhiteSpace(role))
            throw new ValidationDomainException("المسمى الوظيفي مطلوب.");
        if (type == "موظف براتب" && (!salary.HasValue || salary < 0))
            throw new ValidationDomainException("راتب الموظف يجب أن يكون صفرًا أو أكثر.");
        if (type == "متطوع" && salary > 0)
            throw new ValidationDomainException("المتطوع لا يتقاضى راتبًا؛ اختر موظفًا براتب إذا كان له راتب.");
        joinDate = BusinessDate.ValidateTodayOrEarlier(joinDateValue, "تاريخ الانضمام");
    }
}
