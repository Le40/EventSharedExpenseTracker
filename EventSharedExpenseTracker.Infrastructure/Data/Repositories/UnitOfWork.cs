using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;

    public IExpenseRepository Expenses { get; private set; }
    public ITripRepository Trips { get; private set; }
    public IUserRepository Users { get; private set; }
    public IFriendshipRepository Friendships { get; private set; }

    public UnitOfWork(
        ApplicationDbContext context, 
        IExpenseRepository expenseRepository, 
        ITripRepository tripRepository, 
        IUserRepository userRepository, 
        IFriendshipRepository friendshipRepository)
    {
        _context = context;
        Expenses = expenseRepository;
        Trips = tripRepository;
        Users = userRepository;
        Friendships= friendshipRepository;

        Expenses = new ExpenseRepository(_context);
        Trips = new TripRepository(_context);
        Users = new UserRepository(_context);
        Friendships = new FriendshipRepository(_context);
    }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();  
    }
    // DI, so it shouldnt be needed this, as DI container should handle dispose.
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
