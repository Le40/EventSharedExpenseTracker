using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public interface IExpenseService
{
    Task<ServiceResult<List<Expense>>> Index(int tripId, string sortOrder, string searchString, string creator, string categoryFilter);
    Task<ServiceResult<Expense>> Create(int tripId);
    Task<ServiceResult<Expense>> Add(Expense expense, int tripId);
    Task<ServiceResult<Expense>> Get(int id, int tripId);
    Task<ServiceResult<Expense>> Update(Expense expense);
    Task<ServiceResult<Expense>> Delete(int id);
    Task<Expense> LoadParticipants(Expense expense);
}
