using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.Commands;
using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Expenses;

public interface IExpenseService
{
    Task<ServiceResult<TripExpensesQuery>> GetIndex(int tripId, string? sortOrder, string? searchString, bool creator, ExpenseCategory? categoryFilter);
    Task<ServiceResult<ExpenseQuery>> GetExpenseForm(int id);
    Task<ServiceResult<Expense>> Add(ExpenseCommand command, int tripId);
    Task<ServiceResult<Expense>> Update(int id, ExpenseCommand command);
    Task<ServiceResult> Delete(int id);

    Task<ServiceResult<ReceiptParseResult>> ExtractReceiptDataAsync(Stream imageStream);
    Task<ServiceResult<ExpenseCategory>> SuggestCategoryAsync(string expenseName);
}
