using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Friends;

public interface IFriendService
{
    Task<Result<List<Friendship>>> Index();
    Task<Result<List<CustomUser>>> Search (string searchString);
    Task<Result<Friendship>> Invite(int friendId);
    Task<Result<Friendship>> Accept(int friendshipId);
    Task<ServiceResult> Decline(int friendshipId);
    Task<ServiceResult> Delete(int friendshipId);
}
