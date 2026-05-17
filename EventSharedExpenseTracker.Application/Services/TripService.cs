using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Application.Dtos.Mappers;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Application.Services.Utility;
using EventSharedExpenseTracker.Application.Validation;
using EventSharedExpenseTracker.Domain.Models;
using Mapster;

namespace EventSharedExpenseTracker.Application.Services;

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

        var orderBy = TripFilters.GetOrderByExpression(sortOrder);

        var listOfFilters = new List<Func<IQueryable<Trip>, IQueryable<Trip>>>
        {
            TripFilters.Search(searchString),
            //TripFilters.CategoryFilter(categoryFilter)
        };

        // Done like this, so only necessary amount of row is queried from db.
        // And to not have dependency on EF core in application.
        var trips = await _unitOfWork.Trips.GetAllFromUserAsync(userId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        var queries = trips.Select(t => TripMapper.ToQuery(t)).ToList();
            
        return Result<List<TripQuery>>.Ok(queries);
    }

    public async Task<Result<TripDetailsQuery>> Details(int id)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return Result<TripDetailsQuery>.Fail(AppErrors.NotFound<Trip>());

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return Result<TripDetailsQuery>.Fail(AppErrors.Forbidden<Trip>());

        bool canEdit = _authorizationService.AuthorisedToEdit(trip, userId);

        var query = TripMapper.ToDetailsQuery(trip, canEdit, userId);

        return Result<TripDetailsQuery>.Ok(query);
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

        return Result<Trip>.Ok(trip);
    }

    public async Task<Result<TripCommand>> GetForUpdate(int id)
    {
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return Result<TripCommand>.Fail(AppErrors.NotFound<Trip>());

        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return Result<TripCommand>.Fail(AppErrors.Forbidden<Trip>());

        var command = trip.Adapt<TripCommand>();

        return Result<TripCommand>.Ok(command);
    }

    public async Task<Result<Trip>> Update(TripCommand command, Stream? imageFileStream)
    {
        int userId = _requestContext.UserId;
        var existingTrip = await _unitOfWork.Trips.GetByIdAsync(command.Id);
        if (existingTrip == null)
            return Result<Trip>.Fail(AppErrors.NotFound<Trip>());

        if (!_authorizationService.AuthorisedToEdit(existingTrip, userId))
            return Result<Trip>.Fail(AppErrors.Forbidden<Trip>());

        var validationResult = _validationService.ValidateTrip(command);
        if (!validationResult.IsSuccess)
            return Result<Trip>.Fail(validationResult.Errors);

        command.Adapt(existingTrip);

        if (imageFileStream != null)
            existingTrip.ImagePath = await _imageService.SaveImageAsync(imageFileStream, existingTrip.ImagePath ?? string.Empty);


        //_unitOfWork.Trips.Update(existingTrip);
        await _unitOfWork.CompleteAsync();
        return Result<Trip>.Ok(existingTrip);
    }

    public async Task<Result> Delete(int id)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return Result.Fail(AppErrors.NotFound<Trip>());

        int userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return Result.Fail(AppErrors.Forbidden<Trip>());

        var imagePath = trip.ImagePath;

        _unitOfWork.Trips.Delete(trip);
        await _unitOfWork.CompleteAsync();

        if(imagePath != null)
            _imageService.DeleteImageFile(imagePath);

        return Result.Success();
    }

    public async Task<Result<Trip>> AddParticipant(int tripId, int friendId)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);

        if (trip == null)
            return Result<Trip>.Fail(AppErrors.NotFound<Trip>());

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return Result<Trip>.Fail(AppErrors.Forbidden<Trip>());

        // CHECK IF FRIENDSHIP IS CONFIRMED AND EXISTS - LOAD FRIENDSHIP?
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null)
            return Result<Trip>.Fail(AppErrors.NotFound<CustomUser>());

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
        return Result<Trip>.Ok(trip);
    }

    public async Task<Result<Trip>> AddDummy(int id, string participantName)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (trip == null)
            return Result<Trip>.Fail(AppErrors.NotFound<Trip>());

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return Result<Trip>.Fail(AppErrors.Forbidden<Trip>());

        if (trip.Participants.Any(p => p.UserName == participantName))
            return Result<Trip>.Fail(AppErrors.Conflict<TripParticipant>());

        var participant = new TripParticipant()
        {
            TripId = trip.Id,
            UserName = participantName
        };

        trip.Participants.Add(participant);
        await _unitOfWork.CompleteAsync();

        return Result<Trip>.Ok(trip);
    }

    public async Task<Result> DeleteParticipant(int id, int participantId)
    {
        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return Result.Fail(AppErrors.NotFound<Trip>());

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return Result.Fail(AppErrors.Forbidden<Trip>());

        var participant = trip.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null)
            return Result.Fail(AppErrors.NotFound<TripParticipant>());

        // maybe into validation action filter?? to add model error?
        var payments = participant.Payments.Where(p => p.Ammount != null).ToList();
        if (payments.Count != 0)
            return Result.Fail(AppErrors.Validation<TripParticipant>("Participant cant be deleted, he has active expenses."));
        
        trip.Participants.Remove(participant);
        await _unitOfWork.CompleteAsync();

        return Result.Success();
    }
}
