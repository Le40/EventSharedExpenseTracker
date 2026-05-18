using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Authorisation;

namespace EventSharedExpenseTracker.Application.Friends;

public class FriendService : IFriendService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationService _authorizationService;
    private readonly IRequestContext _requestContext;

    public FriendService(IUnitOfWork unitOfWork, IAuthorisationService authorisationService, IRequestContext requestContext)
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
            return AppErrors.NotFound<CustomUser>();

        return user.Friends;
    }

    public async Task<Result<List<CustomUser>>> Search(string? searchString)
    {
        int userId = _requestContext.UserId;
        var user = await _unitOfWork.Users.GetUserWithFriends(userId);

        var options = new FriendshipQueryOptions
        {
            SearchString = searchString,
            Category = ""
        };

        var users = await _unitOfWork.Users.GetAllAsync(userId, options);

        return users;
    }


    public async Task<Result<Friendship>> Invite(int friendId)
    {
        int userId = _requestContext.UserId;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var friend = await _unitOfWork.Users.GetByIdAsync(friendId);
        if (friend == null || user == null)
            return AppErrors.NotFound<Friendship>();

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friendId,
            IsConfirmed = false
        };

        _unitOfWork.Friendships.Create(friendship);
        await _unitOfWork.CompleteAsync();

        return friendship;
    }

    public async Task<Result<Friendship>> Accept(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return AppErrors.NotFound<Friendship>();

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

        return friendshipBack;
    }

    public async Task<Result> Decline(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return AppErrors.NotFound<Friendship>();

        _unitOfWork.Friendships.Delete(friendship);
        await _unitOfWork.CompleteAsync();

        return Result.Ok();
    }

    public async Task<Result> Delete(int friendshipId)
    {
        int userId = _requestContext.UserId;

        var friendship = await _unitOfWork.Friendships.GetByIdAsync(friendshipId);
        if (friendship == null)
            return AppErrors.NotFound<Friendship>();

        _unitOfWork.Friendships.Delete(friendship);
        await _unitOfWork.CompleteAsync();

        return Result.Ok();
    }
}
