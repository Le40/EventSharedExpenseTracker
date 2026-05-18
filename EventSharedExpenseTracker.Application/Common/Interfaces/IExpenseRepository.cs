using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllFromTripAsync(int tripId, ExpenseQueryOptions options);
    Task<Expense?> GetByIdAsync(int id);
    void Add(Expense expense);
    void Update(Expense expense);
    void Delete(Expense expense);
}
