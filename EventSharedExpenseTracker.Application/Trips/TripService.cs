using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Common.Validation;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Models;
using Mapster;
using System.Collections.Generic;

namespace EventSharedExpenseTracker.Application.Trips;

public class TripService : ITripService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationService _authorizationService;
    private readonly IRequestContext _requestContext;
    private readonly IImageService _imageService;
    private readonly IValidationService _validationService;

    public TripService(IUnitOfWork unitOfWork, IAuthorisationService authorisationService, IRequestContext requestContext, IImageService imageService, IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
        _imageService = imageService;
        _validationService = validationService;
    }

    public async Task<Result<List<TripQuery>>> Index(string? sortOrder, string? searchString, string? categoryFilter)
    {
        int userId = _requestContext.UserId;

        var options = new TripQueryOptions
        {
            SearchString = searchString,
            SortBy = sortOrder,
            Category = categoryFilter
        };

        var trips = await _unitOfWork.Trips.GetAllFromUserAsync(userId, options);

        var queries = trips.Select(t => TripMapper.ToQuery(t)).ToList();
            
        return queries;
    }

    public async Task<Result<TripDetailsQuery>> Details(int id)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return AppErrors.Forbidden<Trip>();

        bool canEdit = _authorizationService.AuthorisedToEdit(trip, userId);

        var expenses = trip.Expenses
            .Select(e => {
                var canEditExpense = _authorizationService.AuthorisedToEdit(e, userId);
                return ExpenseMapper.ToQuery(e, canEditExpense);
            })
            .ToList();

        var query = TripMapper.ToDetailsQuery(trip, canEdit, expenses);

        return query;
    }

    public async Task<Result<Trip>> Add(TripCommand command, Stream? imageFileStream)
    {
        var validationResult = _validationService.ValidateTrip(command);
        if (!validationResult.IsSuccess)
            return Result<Trip>.Fail(validationResult.Errors);

        var trip = command.Adapt<Trip>();
        int userId = _requestContext.UserId;
        trip.CreatorId = userId;

        if (imageFileStream != null)
            trip.ImagePath = await _imageService.SaveImageAsync(imageFileStream, string.Empty);

        var participant = new TripParticipant()
        {
            UserId = _requestContext.UserId,
            UserName = _requestContext.UserName
        };

        trip.Participants.Add(participant);
        _unitOfWork.Trips.Add(trip);
        await _unitOfWork.CompleteAsync();

        return trip;
    }

    public async Task<Result<TripQuery>> GetTripForm(int id)
    {
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return AppErrors.Forbidden<Trip>();

        var query = trip.Adapt<TripQuery>();

        return query;
    }

    public async Task<Result<Trip>> Update(int id, TripCommand command, Stream? imageFileStream)
    {
        int userId = _requestContext.UserId;
        var existingTrip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (existingTrip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToEdit(existingTrip, userId))
            return AppErrors.Forbidden<Trip>();

        var validationResult = _validationService.ValidateTrip(command);
        if (!validationResult.IsSuccess)
            return Result<Trip>.Fail(validationResult.Errors);

        command.Adapt(existingTrip);

        if (imageFileStream != null)
            existingTrip.ImagePath = await _imageService.SaveImageAsync(imageFileStream, existingTrip.ImagePath ?? string.Empty);


        //_unitOfWork.Trips.Update(existingTrip);
        await _unitOfWork.CompleteAsync();
        return existingTrip;
    }

    public async Task<Result> Delete(int id)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return AppErrors.NotFound<Trip>();

        int userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return AppErrors.Forbidden<Trip>();

        var imagePath = trip.ImagePath;

        _unitOfWork.Trips.Delete(trip);
        await _unitOfWork.CompleteAsync();

        if(imagePath != null)
            _imageService.DeleteImageFile(imagePath);

        return Result.Ok();
    }

    public async Task<Result<List<TripDetailsQueryarticipant>>> GetParticipants(int id)
    {
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return AppErrors.Forbidden<Trip>();

        var query = trip.Participants.Adapt<List<TripDetailsQueryarticipant>>();

        return query;
    }

    public async Task<Result<Trip>> AddParticipant(int tripId, int friendId)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);

        if (trip == null)
            return AppErrors.NotFound<Trip>();

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return AppErrors.Forbidden<Trip>();

        // CHECK IF FRIENDSHIP IS CONFIRMED AND EXISTS - LOAD FRIENDSHIP?
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null)
            return AppErrors.NotFound<CustomUser>();

        // CHECK IF NOT ALREADY A PARTICIPANT
        // CHECK IF USER IS FRIEND WITH THIS FRIEND


        var participant = new TripParticipant()
        {
            UserId = friend.Id,
            TripId = trip.Id,
            UserName = friend.CustomUserName
        };

        trip.Participants.Add(participant);
        await _unitOfWork.CompleteAsync();
        return trip;
    }

    public async Task<Result<Trip>> AddDummy(int id, string participantName)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return AppErrors.Forbidden<Trip>();

        if (trip.Participants.Any(p => p.UserName == participantName))
            return AppErrors.Conflict<TripParticipant>();

        var participant = new TripParticipant()
        {
            TripId = trip.Id,
            UserName = participantName
        };

        trip.Participants.Add(participant);
        await _unitOfWork.CompleteAsync();

        return trip;
    }

    public async Task<Result> DeleteParticipant(int id, int participantId)
    {
        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return AppErrors.Forbidden<Trip>();

        var participant = trip.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null)
            return AppErrors.NotFound<TripParticipant>();

        //var payments = participant.Payments.Where(p => p.Ammount > 0).ToList();
        if (participant.Payments.Count != 0)
            return Result.Fail(AppErrors.Validation<TripParticipant>("Participant cant be deleted, he has active expenses."));
        
        trip.Participants.Remove(participant);
        await _unitOfWork.CompleteAsync();

        return Result.Ok();
    }
}
