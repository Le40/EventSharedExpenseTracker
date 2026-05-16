using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public interface IExpenseService
{
    Task<Result<List<ExpenseQuery>>> Index(int tripId, string sortOrder, string searchString, bool creator, string categoryFilter);
    Task<Result<Expense>> Add(ExpenseCommand command, int tripId);
    Task<Result<ExpenseCommand>> GetForUpdate(int id);
    Task<Result<Expense>> Update(ExpenseCommand command);
    Task<Result> Delete(int id);
}
