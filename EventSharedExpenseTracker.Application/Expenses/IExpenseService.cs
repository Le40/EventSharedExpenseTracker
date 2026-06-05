using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Expenses;

public interface IExpenseService
{
    Task<ServiceResult<ExpenseIndexQuery>> Index(int tripId, string? sortOrder, string? searchString, bool creator, ExpenseCategory? categoryFilter);
    Task<ServiceResult<Expense>> Add(ExpenseCommand command, int tripId);
    Task<ServiceResult<ExpenseQuery>> GetExpenseForm(int id);
    Task<ServiceResult<Expense>> Update(int id, ExpenseCommand command);
    Task<ServiceResult> Delete(int id);
}
