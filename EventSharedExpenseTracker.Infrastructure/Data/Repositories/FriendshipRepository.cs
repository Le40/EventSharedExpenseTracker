using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly ApplicationDbContext _context;

    public FriendshipRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Friendship?> GetByIdAsync(int id)
    {
        return await _context.Friendships.FindAsync(id);
    }

    public void Create(Friendship friendship)
    {
        _context.Friendships.Add(friendship);
    }

    public void Delete(Friendship friendship)
    {
        _context.Friendships.Remove(friendship);
    }

    public void Update(Friendship friendship)
    {
        _context.Friendships.Update(friendship);
    }
}
