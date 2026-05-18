using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Trips;

public interface ITripService
{
    Task<Result<List<TripQuery>>> Index(string? sortOrder, string? searchString, TripCategory? categoryFilter);
    Task<Result<TripDetailsQuery>> Details(int id);
    Task<Result<Trip>> Add(TripCommand command, Stream? imageFileStream);
    Task<Result<TripQuery>> GetTripForm(int id);
    Task<Result<Trip>> Update(int id, TripCommand command, Stream? imageFileStream);
    Task<Result> Delete(int id);
    Task<Result<List<TripDetailsQueryarticipant>>> GetParticipants(int id);
    Task<Result<Trip>> AddParticipant(int tripId, int id);
    Task<Result<Trip>> AddDummy(int tripId, string partName);
    Task<Result> DeleteParticipant(int tripId, int id);
}
