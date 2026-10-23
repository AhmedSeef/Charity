using Charity.Application.DTOs.Staff;

namespace Charity.Application.Services.Interfaces;

public interface IStaffService
{
    Task<IReadOnlyList<StaffMemberDto>> GetMyStaffAsync();
    Task<StaffMemberDto?> GetByIdAsync(string id);
    Task<StaffMemberDto> CreateAsync(CreateStaffMemberDto dto);
    Task<StaffMemberDto> UpdateAsync(string id, UpdateStaffMemberDto dto);
    Task DeleteAsync(string id);

    Task<IReadOnlyList<StaffUserDto>> GetStaffUsersAsync();
    Task<StaffUserDto> CreateStaffUserAsync(CreateStaffUserDto dto);
    Task<StaffUserDto> UpdateStaffUserPermissionsAsync(string userId, UpdateStaffUserPermissionsDto dto);
    Task ResetStaffUserPasswordAsync(string userId, ChangeStaffUserPasswordDto dto);
    Task DeleteStaffUserAsync(string userId);
}
