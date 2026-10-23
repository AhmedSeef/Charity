using Charity.Application.DTOs.Lookups;

namespace Charity.Application.Services.Interfaces;

public interface IExpenseCategoryService
{
    Task<IReadOnlyList<ExpenseCategoryDto>> GetAllAsync(string? month = null, string? from = null, string? to = null);
    Task<ExpenseCategoryDto> GetByIdAsync(string id, string? month = null, string? from = null, string? to = null);
    Task<IReadOnlyList<ExpenseCategoryDisbursementDetailDto>> GetDisbursementsByCategoryAsync(string categoryId, string? month = null, string? from = null, string? to = null);
    Task<IReadOnlyList<MonthlyExpenseMovementDto>> GetMonthlyMovementsAsync(int? year = null, string? from = null, string? to = null);
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto);
    Task<ExpenseCategoryDto> UpdateAsync(string id, UpdateExpenseCategoryDto dto);
    Task DeleteAsync(string id);
}
