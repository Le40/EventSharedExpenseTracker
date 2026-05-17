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

    public async Task<Result<List<Friendship>>> Index()
    {
        int userId = _requestContext.UserId;

        var user = await _unitOfWork.Users.GetUserWithFriends(userId);
        if (user == null)
            return Result<List<Friendship>>.Fail(AppErrors.NotFound<CustomUser>());

        return Result<List<Friendship>>.Ok(user.Friends);
    }

    public async Task<Result<List<CustomUser>>> Search(string searchString)
    {
        int userId = _requestContext.UserId;
        var user = await _unitOfWork.Users.GetUserWithFriends(userId);

        var listOfFilters = new List<Func<IQueryable<CustomUser>, IQueryable<CustomUser>>>
        {
            //FriendHelper.FriendsFilter(user.Friends),
            FriendFilters.Search(searchString),
            users => users.Where(u => u.Id != userId)
        };

        var users = await _unitOfWork.Users.GetAllAsync(userId,
            filters: listOfFilters.ToArray());

        return Result<List<CustomUser>>.Ok(users);
    }


    public async Task<Result<Friendship>> Invite(int friendId)
    {
        int userId = _requestContext.UserId;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null || user == null)
            return Result<Friendship>.Fail(AppErrors.NotFound<Friendship>());

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friendId,
            IsConfirmed = false
        };

        _unitOfWork.Friendships.Create(friendship);
        await _unitOfWork.CompleteAsync();

        return Result<Friendship>.Ok(friendship);
    }

    public async Task<Result<Friendship>> Accept(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return Result<Friendship>.Fail(AppErrors.NotFound<Friendship>());

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

        return Result<Friendship>.Ok(friendshipBack);
    }

    public async Task<Result> Decline(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return Result.Fail(AppErrors.NotFound<Friendship>());

        _unitOfWork.Friendships.Delete(friendship);
        await _unitOfWork.CompleteAsync();

        return Result.Success();
    }

    public async Task<Result> Delete(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return Result.Fail(AppErrors.NotFound<Friendship>());

        _unitOfWork.Friendships.Delete(friendship);
        await _unitOfWork.CompleteAsync();

        return Result.Success();
    }
}
