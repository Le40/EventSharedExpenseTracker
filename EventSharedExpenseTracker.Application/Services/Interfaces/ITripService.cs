using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public interface ITripService
{
    Task<Result<List<TripQuery>>> Index(string sortOrder, string searchString, string categoryFilter);
    Task<Result<TripDetailsQuery>> Details(int id);
    Task<Result<Trip>> Add(TripCommand command, Stream? imageFileStream);
    Task<Result<TripCommand>> GetForUpdate(int id);
    Task<Result<Trip>> Update(TripCommand command, Stream? imageFileStream);
    Task<Result> Delete(int id);
    Task<Result<Trip>> AddParticipant(int tripId, int id);
    Task<Result<Trip>> AddDummy(int tripId, string partName);
    Task<Result> DeleteParticipant(int tripId, int id);
}
