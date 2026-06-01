using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface ITripRepository
{
    Task<List<Trip>> GetAllFromUserAsync(int userId, TripQueryOptions options);
    Task<Trip?> GetByIdAsync(int id);
    Task<Trip?> GetByIdWithExpensesAsync(int id);
    void Add(Trip trip);
    void Update(Trip trip);
    void Delete(Trip trip);
}
