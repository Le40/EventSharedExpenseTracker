using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Friends;

public interface IFriendService
{
    Task<ServiceResult<List<Friendship>>> Index();
    Task<ServiceResult<List<CustomUser>>> Search (string searchString);
    Task<ServiceResult<Friendship>> Invite(int friendId);
    Task<ServiceResult<Friendship>> Accept(int friendshipId);
    Task<ServiceResult> Decline(int friendshipId);
    Task<ServiceResult> Delete(int friendshipId);
}
