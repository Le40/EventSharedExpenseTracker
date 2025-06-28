
namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IUnitOfWork
{
    IExpenseRepository Expenses { get; }
    ITripRepository Trips { get; }
    IUserRepository Users { get; }
    IFriendshipRepository Friendships { get; }

    Task CompleteAsync();
}
