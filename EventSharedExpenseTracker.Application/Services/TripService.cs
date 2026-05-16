using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Dtos;
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
    private readonly IAuthorisationServ _authorizationService;
    private readonly IRequestContext _requestContext;
    private readonly IImageService _imageService;
    private readonly IValidationService _validationService;

    public TripService(IUnitOfWork unitOfWork, IAuthorisationServ authorisationService, IRequestContext requestContext, IImageService imageService, IValidationService validationService)
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

        var trips = await _unitOfWork.Trips.GetAllFromUserAsync(userId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        var queries = trips.Select(t => new TripQuery
        {
            Id = t.Id,
            Name = t.Name,
            DateFrom = t.DateFrom,
            DateTo = t.DateTo,
            ImagePath = t.ImagePath,
            ParticipantNames = t.Participants
                .Select(p => p.UserName)
                .ToList()
        }).ToList();
            
        return Result<List<TripQuery>>.Ok(queries);
    }

    public async Task<Result<Trip>> Details(int id)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return Result<Trip>.Fail(AppErrors.NotFound<Trip>());

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return Result<Trip>.Fail(AppErrors.Forbidden<Trip>());

        return Result<Trip>.Ok(trip);
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

    public async Task<ServiceResult<Trip>> AddParticipant(int tripId, int friendId)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);

        if (trip == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        // CHECK IF FRIENDSHIP IS CONFIRMED AND EXISTS - LOAD FRIENDSHIP?
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

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
        return new ServiceResult<Trip>("Success", 200);
    }

    public async Task<ServiceResult<Trip>> AddDummy(int id, string participantName)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);
        if (trip == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        if (trip.Participants.Any(p => p.UserName == participantName))
            return new ServiceResult<Trip>("Bad Request.", 400);

        var participant = new TripParticipant()
        {
            TripId = trip.Id,
            UserName = participantName
        };

        trip.Participants.Add(participant);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Trip>("Success", 200);
    }

    public async Task<ServiceResult<Trip>> DeleteParticipant(int id, int participantId)
    {
        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        var participant = trip.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        // maybe into validation action filter?? to add model error?
        var payments = participant.Payments.Where(p => p.Ammount != null).ToList();
        if (payments.Count != 0)
            return new ServiceResult<Trip>("Participant cant be deleted, he has active expenses.", 400);
        
        trip.Participants.Remove(participant);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Trip>("Success", 200);
    }
}
