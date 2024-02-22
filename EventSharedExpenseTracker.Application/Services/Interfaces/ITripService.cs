using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public interface ITripService
{
    Task<ServiceResult<List<Trip>>> Index(string sortOrder, string searchString, string categoryFilter);
    Task<ServiceResult<Trip>> Details(int id);
    Task<ServiceResult<Trip>> Add(Trip trip, Stream? imageFileStream);
    Task<ServiceResult<Trip>> Get(int id);
    Task<ServiceResult<Trip>> Update(Trip trip, Stream? imageFileStream);
    Task<ServiceResult<Trip>> Delete(int id);
    Task<ServiceResult<Trip>> AddParticipant(int tripId, int id);
    Task<ServiceResult<Trip>> AddDummy(int tripId, string partName);
    Task<ServiceResult<Trip>> DeleteParticipant(int tripId, int id);
}
