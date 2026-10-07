using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllFromTripAsync(int tripId, ExpenseFilterOptions options);
    Task<Expense?> GetByIdAsync(int id);
    Task<Expense?> GetByOfflineIdAsync(Guid? offlineId);
    void Add(Expense expense);
    void Update(Expense expense);
    void Delete(Expense expense);
}
