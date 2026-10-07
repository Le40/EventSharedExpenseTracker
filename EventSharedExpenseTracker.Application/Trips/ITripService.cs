using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.Settlements;

namespace EventSharedExpenseTracker.Application.Trips;

public interface ITripService
{
    Task<ServiceResult<List<TripDto>>> GetIndex(string? searchString);
    Task<ServiceResult<TripDetailsQuery>> Details(int id);
    Task<ServiceResult<Trip>> Add(TripDto command, Stream? imageFileStream);
    Task<ServiceResult<TripDto>> GetTripForm(int id);
    Task<ServiceResult<Trip>> Update(int id, TripDto command, Stream? imageFileStream);
    Task<ServiceResult> Delete(int id);
    Task<ServiceResult<TripDto>> GetParticipants(int id);
    Task<ServiceResult<Trip>> AddParticipant(int tripId, int id);
    Task<ServiceResult<Trip>> AddDummy(int tripId, string partName);
    Task<(ServiceResult Result, bool RemovedCurrentUser)> DeleteParticipant(int tripId, int id);
    Task<ServiceResult<List<Settlement>>> GetSettlements(int tripId);

}