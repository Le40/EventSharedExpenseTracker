using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
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

    public async Task<Result<List<TripQuery>>> Index(string? sortOrder, string? searchString, TripCategory? categoryFilter)
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

    public async Task<Result<TripDetailsQuery>> Details(int id)
    {
        int userId = _requestContext.UserId;

        // get Trip
        var trip = await _unitOfWork.Trips.GetByIdWithExpensesAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise for viewing => is user participant of the Trip
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

    public async Task<Result<Trip>> Add(TripCommand command, Stream? imageFileStream)
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

    public async Task<Result<TripQuery>> GetTripForm(int id)
    {
        var userId = _requestContext.UserId;

        // get trip
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        if (!AuthorisationRules.AuthorisedToEdit(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized UPDATE access of trip {TripId}",
                userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        // map 
        //var query = trip.Adapt<TripQuery>();
        var query = TripMapper.ToQuery(trip);

        return query;
    }

    public async Task<Result<Trip>> Update(int id, TripCommand command, Stream? imageFileStream)
    {
        int userId = _requestContext.UserId;

        // validate date range
        if (command.DateFrom > command.DateTo)
            return AppErrors.Validation<Trip>("Date to must be greater than or equal to date from.");

        // get Trip
        var existingTrip = await _unitOfWork.Trips.GetByIdWithExpensesAsync(id);
        if (existingTrip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        if (!AuthorisationRules.AuthorisedToEdit(existingTrip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized UPDATE of trip {TripId}",
                userId, id);
            return AppErrors.Forbidden<Trip>();
        }

        var currencyChanged = existingTrip.BaseCurrencyCode != command.BaseCurrencyCode;

        if (currencyChanged && existingTrip.Expenses.Any())
        {
            return AppErrors.Validation<Trip>(
                "Trip base currency cannot be changed after expenses exist.");
        }

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
        // get Trip
        var trip = await _unitOfWork.Trips.GetByIdWithExpensesAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        int userId = _requestContext.UserId;
        if (!AuthorisationRules.AuthorisedToEdit(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized UPDATE of trip {TripId}",
                userId, id);
            return AppErrors.Forbidden<Trip>();
        }

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

    public async Task<Result<TripDetailsQuery>> GetParticipants(int id)
    {
        var userId = _requestContext.UserId;

        // get trip
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        if (!AuthorisationRules.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted to GET participants of trip {TripId} unauthorised",
                 userId, id);
            return AppErrors.Forbidden<Trip>();
        }

        // map
        var query = trip.Adapt<TripDetailsQuery>();

        return query;
    }

    public async Task<Result<Trip>> AddParticipant(int tripId, int friendId)
    {
        // get trip
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        var userId = _requestContext.UserId;
        if (!AuthorisationRules.AuthorisedToEdit(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted to CREATE participant of trip {TripId} unauthorised.",
                userId, tripId);
            return AppErrors.Forbidden<Trip>();
        }

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

    public async Task<Result<Trip>> AddDummy(int id, string participantName)
    {
        // get Trip
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        var userId = _requestContext.UserId;
        if (!AuthorisationRules.AuthorisedToEdit(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted to CREATE dummy participant of trip {TripId} unauthorised.",
                userId, id);
            return AppErrors.Forbidden<Trip>();
        }

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
        // get Trip
        var trip = await _unitOfWork.Trips.GetByIdWithExpensesAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise
        var userId = _requestContext.UserId;
        if (!AuthorisationRules.AuthorisedToEdit(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted to DELETE participant of trip {TripId} unauthorised.",
                userId, id);
            return AppErrors.Forbidden<Trip>();
        }

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
}
