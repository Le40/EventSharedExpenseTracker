using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllFromTripAsync(int tripId,
        Func<IQueryable<Expense>, IOrderedQueryable<Expense>> orderBy,
        params Func<IQueryable<Expense>, IQueryable<Expense>>[] filters);
    Task<Expense?> GetByIdAsync(int id);
    void Add(Expense expense);
    void Update(Expense expense);
    void Delete(Expense expense);
}
