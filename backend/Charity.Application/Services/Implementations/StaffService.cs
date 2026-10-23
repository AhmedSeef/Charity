using Charity.Application.DTOs.Staff;
using Charity.Application.Mappers;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Domain.Interfaces;
using Charity.Domain.Security;
using Charity.Domain.Validation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Charity.Application.Services.Implementations;

public class StaffService : IStaffService
{
    private readonly IRepository<StaffMember> _staffRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffService(
        IRepository<StaffMember> staffRepo,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _staffRepo = staffRepo;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    private void EnsureStaffManagePermission()
    {
        if (!_currentUserService.HasPermission(AppPermissions.StaffManage))
        {
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية إدارة فريق العمل والمستخدمين.");
        }
    }

    private List<string> ResolvePermissions(string template, List<string>? requested)
    {
        if (!JobRoleTemplates.TemplatePermissions.TryGetValue(template, out var defaults))
            throw new ValidationDomainException("القالب الوظيفي غير معروف.");

        var permissions = requested is { Count: > 0 } ? requested.Distinct(StringComparer.Ordinal).ToList() : defaults.ToList();
        if (permissions.Any(p => !AppPermissions.All.Contains(p, StringComparer.Ordinal)))
            throw new ValidationDomainException("توجد صلاحية غير معروفة.");
        if (_currentUserService.Role != "CharityManager" && _currentUserService.Role != "Admin" &&
            permissions.Any(p => !_currentUserService.HasPermission(p)))
            throw new UnauthorizedDomainException("لا يمكنك منح صلاحية لا تملكها.");

        return permissions;
    }

    public async Task<IReadOnlyList<StaffMemberDto>> GetMyStaffAsync()
    {
        EnsureStaffManagePermission();
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var staff = await _staffRepo.FindAsync(s => s.CharityId == charityId);
        return staff.Select(s => s.ToDto()).ToList();
    }

    public async Task<StaffMemberDto?> GetByIdAsync(string id)
    {
        EnsureStaffManagePermission();
        var charityId = _currentUserService.CharityId ?? string.Empty;
        var member = await _staffRepo.GetByIdAsync(id);
        if (member == null || member.CharityId != charityId) return null;
        return member.ToDto();
    }

    public async Task<StaffMemberDto> CreateAsync(CreateStaffMemberDto dto)
    {
        EnsureStaffManagePermission();
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
        EnsureStaffManagePermission();
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
        EnsureStaffManagePermission();
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

    public async Task<IReadOnlyList<StaffUserDto>> GetStaffUsersAsync()
    {
        EnsureStaffManagePermission();
        var charityId = TenantAccess.RequireCharity(_currentUserService);
        var users = _userManager.Users.Where(u => u.CharityId == charityId).ToList();
        var result = new List<StaffUserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Staff"))
                continue;

            var claims = await _userManager.GetClaimsAsync(user);
            var template = claims.FirstOrDefault(c => c.Type == "role_template")?.Value ?? JobRoleTemplates.Custom;
            var perms = claims.Where(c => c.Type == "permission").Select(c => c.Value).Distinct().ToList();

            result.Add(new StaffUserDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                FullName = user.FullName,
                CharityId = user.CharityId ?? string.Empty,
                Role = "Staff",
                RoleTemplate = template,
                Permissions = perms,
                CreatedAt = user.CreatedAt,
                CreatedByName = user.CreatedByName,
                UpdatedAt = user.UpdatedAt,
                UpdatedByName = user.UpdatedByName
            });
        }

        return result;
    }

    public async Task<StaffUserDto> CreateStaffUserAsync(CreateStaffUserDto dto)
    {
        EnsureStaffManagePermission();
        var charityId = TenantAccess.RequireCharity(_currentUserService);

        var username = dto.Username?.Trim() ?? string.Empty;
        var fullName = dto.FullName?.Trim() ?? string.Empty;
        var password = dto.Password?.Trim() ?? string.Empty;

        if (username.Length < 3)
            throw new ValidationDomainException("اسم المستخدم يجب أن يتكون من 3 أحرف على الأقل.");
        if (fullName.Length == 0)
            throw new ValidationDomainException("الاسم الكامل للموظف مطلوب.");
        if (password.Length < 4)
            throw new ValidationDomainException("كلمة المرور يجب ألا تقل عن 4 خانات.");

        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null)
            throw new ValidationDomainException("اسم المستخدم هذا مستخدم بالفعل، يرجى اختيار اسم آخر.");

        var template = string.IsNullOrWhiteSpace(dto.RoleTemplate) ? JobRoleTemplates.Custom : dto.RoleTemplate;
        var effectivePermissions = ResolvePermissions(template, dto.Permissions);

        var user = new ApplicationUser
        {
            UserName = username,
            FullName = fullName,
            CharityId = charityId,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new ValidationDomainException(string.Join(" ", result.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, "Staff");
        if (!roleResult.Succeeded)
            throw new ValidationDomainException(string.Join(" ", roleResult.Errors.Select(e => e.Description)));

        var claimsToAdd = new List<Claim> { new("role_template", template) };


        foreach (var perm in effectivePermissions)
        {
            claimsToAdd.Add(new Claim("permission", perm));
        }

        var claimsResult = await _userManager.AddClaimsAsync(user, claimsToAdd);
        if (!claimsResult.Succeeded)
            throw new ValidationDomainException(string.Join(" ", claimsResult.Errors.Select(e => e.Description)));

        return new StaffUserDto
        {
            Id = user.Id,
            Username = user.UserName,
            FullName = user.FullName,
            CharityId = user.CharityId,
            Role = "Staff",
            RoleTemplate = template,
            Permissions = effectivePermissions,
            CreatedAt = user.CreatedAt,
            CreatedByName = user.CreatedByName,
            UpdatedAt = user.UpdatedAt,
            UpdatedByName = user.UpdatedByName
        };
    }

    public async Task<StaffUserDto> UpdateStaffUserPermissionsAsync(string userId, UpdateStaffUserPermissionsDto dto)
    {
        EnsureStaffManagePermission();
        var charityId = TenantAccess.RequireCharity(_currentUserService);

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new EntityNotFoundException("User", userId);

        if (user.CharityId != charityId)
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك تعديل صلاحيات موظف لجمعية أخرى!");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff") || roles.Contains("Admin") || roles.Contains("CharityManager"))
            throw new ValidationDomainException("لا يمكن تعديل صلاحيات مدير النظام أو مدير الجمعية عبر هذه الشاشة.");

        var template = string.IsNullOrWhiteSpace(dto.RoleTemplate) ? JobRoleTemplates.Custom : dto.RoleTemplate;
        var effectivePermissions = ResolvePermissions(template, dto.Permissions);
        var currentClaims = await _userManager.GetClaimsAsync(user);
        var claimsToRemove = currentClaims.Where(c => c.Type == "permission" || c.Type == "role_template").ToList();
        if (claimsToRemove.Any())
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, claimsToRemove);
            if (!removeResult.Succeeded)
                throw new ValidationDomainException(string.Join(" ", removeResult.Errors.Select(e => e.Description)));
        }

        var claimsToAdd = new List<Claim> { new("role_template", template) };


        foreach (var perm in effectivePermissions)
        {
            claimsToAdd.Add(new Claim("permission", perm));
        }

        var claimsResult = await _userManager.AddClaimsAsync(user, claimsToAdd);
        if (!claimsResult.Succeeded)
            throw new ValidationDomainException(string.Join(" ", claimsResult.Errors.Select(e => e.Description)));
        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
            throw new ValidationDomainException(string.Join(" ", stampResult.Errors.Select(e => e.Description)));

        return new StaffUserDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            FullName = user.FullName,
            CharityId = user.CharityId ?? string.Empty,
            Role = "Staff",
            RoleTemplate = template,
            Permissions = effectivePermissions,
            CreatedAt = user.CreatedAt,
            CreatedByName = user.CreatedByName,
            UpdatedAt = user.UpdatedAt,
            UpdatedByName = user.UpdatedByName
        };
    }

    public async Task ResetStaffUserPasswordAsync(string userId, ChangeStaffUserPasswordDto dto)
    {
        if (_currentUserService.Role != "CharityManager")
            throw new UnauthorizedDomainException("إعادة تعيين كلمة مرور الموظف متاحة لمدير الجمعية فقط.");

        var actor = await _userManager.FindByIdAsync(_currentUserService.UserId ?? string.Empty);
        if (actor == null || string.IsNullOrWhiteSpace(actor.CharityId) ||
            actor.CharityId != _currentUserService.CharityId ||
            !await _userManager.IsInRoleAsync(actor, "CharityManager"))
            throw new UnauthorizedDomainException("إعادة تعيين كلمة مرور الموظف متاحة لمدير الجمعية فقط.");

        var charityId = actor.CharityId;
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new EntityNotFoundException("User", userId);

        if (user.CharityId != charityId)
            throw new UnauthorizedDomainException("لا يمكنك إعادة تعيين كلمة مرور موظف جمعية أخرى.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff") || roles.Contains("Admin") || roles.Contains("CharityManager"))
            throw new ValidationDomainException("يمكن إعادة تعيين كلمة مرور حساب الموظف فقط.");

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Trim().Length < 4)
            throw new ValidationDomainException("كلمة المرور الجديدة يجب ألا تقل عن 4 خانات.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            throw new ValidationDomainException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeleteStaffUserAsync(string userId)
    {
        EnsureStaffManagePermission();
        var charityId = TenantAccess.RequireCharity(_currentUserService);

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new EntityNotFoundException("User", userId);

        if (user.CharityId != charityId)
            throw new UnauthorizedDomainException("غير مصرح: لا يمكنك حذف موظف لجمعية أخرى!");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin") || roles.Contains("CharityManager"))
            throw new ValidationDomainException("لا يمكن حذف حساب مدير النظام أو مدير الجمعية.");

        await _userManager.DeleteAsync(user);
    }
}
