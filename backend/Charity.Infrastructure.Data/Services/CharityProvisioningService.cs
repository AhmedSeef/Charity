using Charity.Application.DTOs.Dashboard;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Exceptions;
using Charity.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Charity.Infrastructure.Data.Services;

public sealed class CharityProvisioningService(
    CharityDbContext db,
    UserManager<ApplicationUser> users) : ICharityProvisioningService
{
    public async Task<CharityDto> CreateAsync(CreateCharityDto request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
            throw new ValidationDomainException("اسم الجمعية مطلوب.");
        await ValidateManagerAsync(request.ManagerUsername, request.ManagerFullName);
        if (await db.Charities.AnyAsync(c => c.Name == name))
            throw new ValidationDomainException("اسم الجمعية موجود بالفعل.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var charity = new Charity.Domain.Entities.Charity
        {
            Name = name,
            RoleDesc = request.RoleDesc.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            License = request.License.Trim(),
            GovernorateId = string.IsNullOrWhiteSpace(request.GovernorateId) ? null : request.GovernorateId.Trim(),
            GovernorateName = string.IsNullOrWhiteSpace(request.GovernorateName) ? null : request.GovernorateName.Trim(),
            DistrictId = string.IsNullOrWhiteSpace(request.DistrictId) ? null : request.DistrictId.Trim(),
            DistrictName = string.IsNullOrWhiteSpace(request.DistrictName) ? null : request.DistrictName.Trim()
        };
        db.Charities.Add(charity);
        await db.SaveChangesAsync();

        await AddManagerAsync(charity.Id, request.ManagerUsername, request.ManagerPassword, request.ManagerFullName);
        await transaction.CommitAsync();
        return ToDto(charity);
    }

    public async Task CreateManagerAsync(string charityId, CreateCharityManagerDto request)
    {
        if (!await db.Charities.AnyAsync(c => c.Id == charityId))
            throw new EntityNotFoundException("Charity", charityId);
        await ValidateManagerAsync(request.Username, request.FullName);

        var managerRoleId = await db.Roles.Where(r => r.Name == "CharityManager").Select(r => r.Id).FirstAsync();
        var hasManager = await (from user in db.Users
                                join assignment in db.UserRoles on user.Id equals assignment.UserId
                                where user.CharityId == charityId && assignment.RoleId == managerRoleId
                                select user.Id).AnyAsync();
        if (hasManager)
            throw new ValidationDomainException("هذه الجمعية لديها مستخدم أساسي بالفعل.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        await AddManagerAsync(charityId, request.Username, request.Password, request.FullName);
        await transaction.CommitAsync();
    }

    private async Task ValidateManagerAsync(string username, string fullName)
    {
        if (username.Trim().Length < 3 || fullName.Trim().Length == 0)
            throw new ValidationDomainException("اسم المستخدم والاسم الكامل مطلوبان.");
        if (await users.FindByNameAsync(username.Trim()) != null)
            throw new ValidationDomainException("اسم المستخدم موجود بالفعل.");
    }

    private async Task AddManagerAsync(string charityId, string username, string password, string fullName)
    {
        var manager = new ApplicationUser
        {
            UserName = username.Trim(),
            FullName = fullName.Trim(),
            CharityId = charityId,
            EmailConfirmed = true
        };
        var created = await users.CreateAsync(manager, password);
        if (!created.Succeeded)
            throw new ValidationDomainException(string.Join(" ", created.Errors.Select(e => e.Description)));
        var assigned = await users.AddToRoleAsync(manager, "CharityManager");
        if (!assigned.Succeeded)
            throw new ValidationDomainException(string.Join(" ", assigned.Errors.Select(e => e.Description)));
    }

    private static CharityDto ToDto(Charity.Domain.Entities.Charity charity) => new()
    {
        Id = charity.Id, Name = charity.Name, RoleDesc = charity.RoleDesc,
        Phone = charity.Phone, Address = charity.Address, License = charity.License,
        Color = charity.Color, Icon = charity.Icon, Logo = charity.Logo,
        GovernorateId = charity.GovernorateId,
        GovernorateName = charity.GovernorateName,
        DistrictId = charity.DistrictId,
        DistrictName = charity.DistrictName
    };
}
