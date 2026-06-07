using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.Settlements;

namespace EventSharedExpenseTracker.Application.Trips;

public interface ITripService
{
    Task<ServiceResult<List<TripQuery>>> GetIndex(string? sortOrder, string? searchString, TripCategory? categoryFilter);
    Task<ServiceResult<TripDetailsQuery>> Details(int id);
    Task<ServiceResult<Trip>> Add(TripCommand command, Stream? imageFileStream);
    Task<ServiceResult<TripQuery>> GetTripForm(int id);
    Task<ServiceResult<Trip>> Update(int id, TripCommand command, Stream? imageFileStream);
    Task<ServiceResult> Delete(int id);
    Task<ServiceResult<TripParticipantsQuery>> GetParticipants(int id);
    Task<ServiceResult<Trip>> AddParticipant(int tripId, int id);
    Task<ServiceResult<Trip>> AddDummy(int tripId, string partName);
    Task<ServiceResult> DeleteParticipant(int tripId, int id);
    Task<ServiceResult<List<Settlement>>> GetSettlements(int tripId);

}