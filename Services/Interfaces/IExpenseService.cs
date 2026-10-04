using Tricount.DTOs.Requests;
using Tricount.DTOs.Responses;

namespace Tricount.Services.Interfaces;

public interface IExpenseService
{
    Task<ExpenseResponse?> GetByIdAsync(int id);
    Task<IEnumerable<ExpenseResponse>> GetAllAsync();
    Task<IEnumerable<ExpenseResponse>> GetByGroupIdAsync(int groupId);
    Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request);
    Task<ExpenseResponse> UpdateAsync(int id, UpdateExpenseRequest request);
    Task<bool> DeleteAsync(int id);
}
