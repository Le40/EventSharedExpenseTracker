using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.Settlements;
using Mapster;
using Microsoft.Extensions.Logging;

namespace EventSharedExpenseTracker.Application.Trips;

public class TripService : ITripService
{
    private readonly ILogger<TripService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestContext _requestContext;
    private readonly IImageService _imageService;

    public TripService(ILogger<TripService> logger, IUnitOfWork unitOfWork,  IRequestContext requestContext, IImageService imageService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _requestContext = requestContext;
        _imageService = imageService;
    }

    public async Task<ServiceResult<List<TripQuery>>> GetIndex(string? sortOrder, string? searchString, TripCategory? categoryFilter)
    {
        int userId = _requestContext.UserId;

        // options for query
        var options = new TripQueryOptions
        {
            SearchString = searchString,
            SortBy = sortOrder,
            Category = categoryFilter
        };

        // get Trips
        var trips = await _unitOfWork.Trips.GetAllFromUserAsync(userId, options);

        // map to query/response
        var queries = trips.Select(t => TripMapper.ToQuery(t)).ToList();
            
        return queries;
    }

    public async Task<ServiceResult<TripDetailsQuery>> Details(int id)
    {
        // get and autorise trip
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdWithExpensesAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!AuthorisationRules.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId}",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        // authorise for editing => is user creator of the Trip
        bool canUserEdit = AuthorisationRules.AuthorisedToEdit(trip, userId);

        // map Expenses to query/respone + authorise each Expense
        var expenseQueries = trip.Expenses
            .Select(e => {
                var canUserEditExpense = AuthorisationRules.AuthorisedToEdit(e, userId);
                return ExpenseMapper.ToQuery(e, canUserEditExpense);
            }).ToList();

        // map trip to query/response
        var query = TripMapper.ToDetailsQuery(trip, canUserEdit, expenseQueries);

        return query;
    }

    public async Task<ServiceResult<Trip>> Add(TripCommand command, Stream? imageFileStream)
    {
        int userId = _requestContext.UserId;

        // validate date range
        if (command.DateFrom > command.DateTo)
            return AppErrors.Validation<Trip>("Date to must be greater than or equal to date from.");

        // map command to Trip 
        var trip = command.Adapt<Trip>();
        // set current user as creator
        trip.CreatorId = userId;

        // add current user to the Trip
        var participantResult = trip.AddParticipant(userId, _requestContext.UserName);
        if (!participantResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(participantResult.Errors);

        // process images
        if (imageFileStream != null)
            trip.ImagePath = await _imageService.SaveImageAsync(imageFileStream, string.Empty);

        // create trip
        _unitOfWork.Trips.Add(trip);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Trip {TripId} created by user {UserId}",
            trip.Id,
            userId);

        return trip;
    }

    public async Task<ServiceResult<TripQuery>> GetTripForm(int id)
    {
        // get and autorise trip
        int userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForEdit(id);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to update trip {TripId} without permission.",
                userId, id);
            return tripResult.ToFailure<TripQuery>();
        }

        var trip = tripResult.Value!;

        // map 
        //var query = trip.Adapt<TripQuery>();
        var query = TripMapper.ToQuery(trip);

        return query;
    }

    public async Task<ServiceResult<Trip>> Update(int id, TripCommand command, Stream? imageFileStream)
    {
        int userId = _requestContext.UserId;

        // validate date range
        if (command.DateFrom > command.DateTo)
            return AppErrors.Validation<Trip>("Date to must be greater than or equal to date from.");

        // get and autorise trip
        var tripResult = await GetTripAuthorisedForEdit(id);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to update trip {TripId} without permission.",
                userId, id);
            return tripResult;
        }

        var existingTrip = tripResult.Value!;

        var currencyResult = existingTrip.ChangeCurrency(command.BaseCurrencyCode);
        if (!currencyResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(currencyResult.Errors);

        // saving old image path, cause adapt rewrites it with null if it hasnt changed.
        var oldImagePath = existingTrip.ImagePath;
        command.Adapt(existingTrip);
        existingTrip.ImagePath = oldImagePath;

        // process images
        if (imageFileStream != null)
            existingTrip.ImagePath = await _imageService.SaveImageAsync(imageFileStream, existingTrip.ImagePath ?? string.Empty);

        //_unitOfWork.Trips.Update(existingTrip);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Trip {TripId} updated by user {UserId}",
            id,
            userId);

        return existingTrip;
    }

    public async Task<ServiceResult> Delete(int id)
    {
        // get and autorise trip
        int userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForEdit(id);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to delete trip {TripId} without permission.",
                userId, id);
            return tripResult;
        }

        var trip = tripResult.Value!;

        // get image path for deletion
        var imagePath = trip.ImagePath;

        _unitOfWork.Trips.Delete(trip);
        await _unitOfWork.CompleteAsync();

        // delete image after deleting trip
        if(imagePath != null)
            _imageService.DeleteImageFile(imagePath);

        _logger.LogInformation("Trip {TripId} deleted by user {UserId}",
            id,
            userId);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<TripParticipantsQuery>> GetParticipants(int id)
    {
        // get and autorise trip
        var userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForView(id);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to get participants from trip {TripId} without permission.",
                userId, id);
            return tripResult.ToFailure<TripParticipantsQuery>();
        }

        var trip = tripResult.Value!;

        // map
        var query = trip.Adapt<TripParticipantsQuery>();

        return query;
    }

    public async Task<ServiceResult<Trip>> AddParticipant(int tripId, int friendId)
    {
        // get and autorise trip
        var userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForEdit(tripId);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to create participant from trip {TripId} without permission.",
                userId, tripId);
            return tripResult;
        }

        var trip = tripResult.Value!;

        // CHECK IF FRIENDSHIP IS CONFIRMED AND EXISTS - LOAD FRIENDSHIP?
        // FOR NOW: get other user, not yet friend
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null)
            return AppErrors.NotFound<CustomUser>();

        // CHECK IF NOT ALREADY A PARTICIPANT
        // CHECK IF USER IS FRIEND WITH THIS FRIEND

        // add participant to trip
        var participantResult = trip.AddParticipant(friendId, friend.CustomUserName);
        if (!participantResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(participantResult.Errors);

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("User {FriendId} added to trip {TripId} by user {UserId}",
            friend.Id, 
            tripId,
            userId);

        return trip;
    }

    public async Task<ServiceResult<Trip>> AddDummy(int id, string participantName)
    {
        // get and autorise trip
        var userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForEdit(id);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to create participant from trip {TripId} without permission.",
                userId, id);
            return tripResult;
        }

        var trip = tripResult.Value!;

        // add participant to trip
        var participantResult = trip.AddDummyParticipant(participantName);
        if (!participantResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(participantResult.Errors);

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Dummy participant {ParticipantName} added to trip {TripId} by user {UserId}",
            participantName,
            id,
            userId);

        return trip;
    }

    public async Task<ServiceResult> DeleteParticipant(int id, int participantId)
    {
        // get and autorise trip
        var userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForEdit(id);

        if (!tripResult.IsSuccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to delete participant from trip {TripId} without permission.",
                userId, id);
            return tripResult;
        }

        var trip = tripResult.Value!;

        // remove participant 
        var participantResult = trip.RemoveParticipant(participantId);
        if (!participantResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(participantResult.Errors);

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Participant {ParticipantId} deleted from trip {TripId} by user {UserId}",
            participantId,
            id,
            userId);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<Settlement>>> GetSettlements(int tripId)
    {
        var tripResult = await GetTripAuthorisedForView(tripId);

        if (!tripResult.IsSuccess)
            return tripResult.ToFailure<List<Settlement>>();

        var trip = await _unitOfWork.Trips.GetByIdWithExpensesAsync(tripId);

        if (trip == null)
            return AppErrors.NotFound<Trip>();

        return SettlementCalculator.Calculate(trip);
    }

    private async Task<ServiceResult<Trip>> GetTripAuthorisedForEdit (int tripId)
    {
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!AuthorisationRules.AuthorisedToEdit(trip, userId))
            return AppErrors.Forbidden<Trip>();

        return trip;
    }

    private async Task<ServiceResult<Trip>> GetTripAuthorisedForView(int tripId)
    {
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!AuthorisationRules.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId}",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        return trip;
    }

}
