using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventSharedExpenseTracker.Application.Friends;

public class FriendService : IFriendService
{
    private readonly ILogger<FriendService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestContext _requestContext;

    public FriendService(ILogger<FriendService> logger, IUnitOfWork unitOfWork, IRequestContext requestContext)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _requestContext = requestContext;
    }

    public async Task<Result<List<Friendship>>> Index()
    {
        int userId = _requestContext.UserId;

        var user = await _unitOfWork.Users.GetUserWithFriends(userId);
        if (user == null)
            return AppErrors.NotFound<CustomUser>();

        return (Result<List<Friendship>>)user.Friends;
    }

    public async Task<Result<List<CustomUser>>> Search(string? searchString)
    {
        int userId = _requestContext.UserId;
        //var user = await _unitOfWork.Users.GetUserWithFriends(userId);

        var options = new FriendshipQueryOptions
        {
            SearchString = searchString,
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

        _logger.LogInformation("Invite {InviteId} created for user {FriendId} by user {UserId}",
            friendship.Id,
            friend,
            userId);

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

        _logger.LogInformation("Invite {InviteId} accepted for user {FriendId} by user {UserId}",
            friendship.Id,
            friendship.FriendId,
            userId);

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

        _logger.LogInformation("Invite {InviteId} declined for user {FriendId} by user {UserId}",
            friendship.Id,
            friendship.FriendId,
            userId);

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

        _logger.LogInformation("Invite {InviteId} deleted for user {FriendId} by user {UserId}",
            friendship.Id,
            friendship.FriendId,
            userId);

        return Result.Ok();
    }
}
