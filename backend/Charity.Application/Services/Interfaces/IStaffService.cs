using Charity.Application.DTOs.Staff;

namespace Charity.Application.Services.Interfaces;

public interface IStaffService
{
    Task<IReadOnlyList<StaffMemberDto>> GetMyStaffAsync();
    Task<StaffMemberDto?> GetByIdAsync(string id);
    Task<StaffMemberDto> CreateAsync(CreateStaffMemberDto dto);
    Task<StaffMemberDto> UpdateAsync(string id, UpdateStaffMemberDto dto);
    Task DeleteAsync(string id);
}
