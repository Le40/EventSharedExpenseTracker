using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Interfaces;

public interface ITripRepository
{
    Task<List<Trip>> GetAllFromUserAsync(int userId,
        Func<IQueryable<Trip>, IOrderedQueryable<Trip>> orderBy,
        params Func<IQueryable<Trip>, IQueryable<Trip>>[] filters);
    Task<Trip?> GetByIdAsync(int id);
    Task<Trip?> GetByIdWithExpenses(int id);
    void Add(Trip trip);
    void Update(Trip trip);
    void Delete(Trip trip);
}
