using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Application.Services.Utility;

namespace EventSharedExpenseTracker.Application.Services;

public class TripService : ITripService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationServ _authorizationService;
    private readonly IRequestContext _requestContext;
    private readonly IImageService _imageService;

    public TripService(IUnitOfWork unitOfWork, IAuthorisationServ authorisationService, IRequestContext requestContext, IImageService imageService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
        _imageService = imageService;
    }


    public async Task<ServiceResult<List<Trip>>> Index(string sortOrder, string searchString, string categoryFilter)
    {
        int userId = _requestContext.UserId;

        var orderBy = TripHelper.GetOrderByExpression(sortOrder);

        var listOfFilters = new List<Func<IQueryable<Trip>, IQueryable<Trip>>>
        {
            TripHelper.Search(searchString),
            //TripHelper.CategoryFilter(categoryFilter)
        };

        var trips = await _unitOfWork.Trips.GetAllFromUserAsync(userId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        return new ServiceResult<List<Trip>>(trips, 200);
    }

    public async Task<ServiceResult<Trip>> Details(int id)
    {
        var trip = await _unitOfWork.Trips.GetByIdWithExpenses(id);
        if (trip == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToView(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        return new ServiceResult<Trip>(trip, 200);
    }

    public async Task<ServiceResult<Trip>> Get(int id)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        var userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        return new ServiceResult<Trip>(trip, 200);
    }

    public async Task<ServiceResult<Trip>> Add(Trip trip, Stream? imageFileStream)
    {
        if (imageFileStream != null)
            trip.ImagePath = await _imageService.UpdateImageAsync(imageFileStream, string.Empty);

        if (_requestContext.UserId <= 0)
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        var participant = new TripParticipant()
        {
            UserId = _requestContext.UserId,
            TripId = trip.Id,
            UserName = _requestContext.UserName
        };

        trip.Participants.Add(participant);
        _unitOfWork.Trips.Add(trip);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Trip>("Success", 200);
    }

    public async Task<ServiceResult<Trip>> Update(Trip trip, Stream? imageFileStream)
    {
        if (imageFileStream != null)
            trip.ImagePath = await _imageService.UpdateImageAsync(imageFileStream, trip.ImagePath ?? string.Empty);

        int userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        _unitOfWork.Trips.Update(trip);
        await _unitOfWork.CompleteAsync();
        return new ServiceResult<Trip>("Success", 200);
    }

    public async Task<ServiceResult<Trip>> Delete(int id)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(id);

        if (trip == null)
            return new ServiceResult<Trip>("Needed resource not found.", 404);

        int userId = _requestContext.UserId;
        if (!_authorizationService.AuthorisedToEdit(trip, userId))
            return new ServiceResult<Trip>("Insufficient permissions.", 403);

        if (trip.ImagePath != null)
            _imageService.DeleteImageFile(trip.ImagePath);

        _unitOfWork.Trips.Delete(trip);
        await _unitOfWork.CompleteAsync();
        return new ServiceResult<Trip>("Success", 200);
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
