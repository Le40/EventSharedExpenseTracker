using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Expenses;

public interface IExpenseService
{
    Task<Result<List<ExpenseQuery>>> Index(int tripId, string? sortOrder, string? searchString, bool creator, ExpenseCategory? categoryFilter);
    Task<Result<Expense>> Add(ExpenseCommand command, int tripId);
    Task<Result<ExpenseQuery>> GetExpenseForm(int id);
    Task<Result<Expense>> Update(int id, ExpenseCommand command);
    Task<Result> Delete(int id);
}
