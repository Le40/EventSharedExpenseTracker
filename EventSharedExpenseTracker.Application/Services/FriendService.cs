using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Application.Services.Utility;

namespace EventSharedExpenseTracker.Application.Services;

public class FriendService : IFriendService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationServ _authorizationService;
    private readonly IRequestContext _requestContext;

    public FriendService(IUnitOfWork unitOfWork, IAuthorisationServ authorisationService, IRequestContext requestContext)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
    }

    public async Task<ServiceResult<List<Friendship>>> Index()
    {
        int userId = _requestContext.UserId;

        var user = await _unitOfWork.Users.GetUserWithFriends(userId);
        if (user == null)
            return new ServiceResult<List<Friendship>>("Needed resource not found.", 404);

        return new ServiceResult<List<Friendship>>(user.Friends, 200);
    }

    public async Task<ServiceResult<List<CustomUser>>> Search(string searchString)
    {
        int userId = _requestContext.UserId;
        var user = await _unitOfWork.Users.GetUserWithFriends(userId);

        var listOfFilters = new List<Func<IQueryable<CustomUser>, IQueryable<CustomUser>>>
        {
            //FriendHelper.FriendsFilter(user.Friends),
            FriendHelper.Search(searchString)
        };

        var users = await _unitOfWork.Users.GetAllAsync(userId,
            filters: listOfFilters.ToArray());

        return new ServiceResult<List<CustomUser>>(users, 200);
    }


    public async Task<ServiceResult<Friendship>> Invite(int friendId)
    {
        int userId = _requestContext.UserId;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null || user == null)
            return new ServiceResult<Friendship>("Needed resource not found.", 404);

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friendId,
            IsConfirmed = false
        };

        _unitOfWork.Friendships.Create(friendship);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Friendship>("Success", 200);
    }

    public async Task<ServiceResult<Friendship>> Accept(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return new ServiceResult<Friendship>("Needed resource not found.", 404);

        friendship.IsConfirmed = true;
        _unitOfWork.Friendships.Update(friendship);

        var friendshipBack = new Friendship
        {
            UserId = userId,
            FriendId = friendship.FriendId,
            IsConfirmed = true
        };
        _unitOfWork.Friendships.Create(friendshipBack);

        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Friendship>("Success", 200);
    }

    public async Task<ServiceResult<Friendship>> Decline(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return new ServiceResult<Friendship>("Needed resource not found.", 404);

        _unitOfWork.Friendships.Delete(friendship);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Friendship>("Success", 200);
    }

    public async Task<ServiceResult<Friendship>> Delete(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return new ServiceResult<Friendship>("Needed resource not found.", 404);

        _unitOfWork.Friendships.Delete(friendship);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Friendship>("Success", 200);
    }
}
