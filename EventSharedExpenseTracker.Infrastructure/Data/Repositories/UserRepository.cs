using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Friends;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomUser>> GetAllAsync(int userId, FriendshipQueryOptions options)
    {
        // DEFAULT MANDATORY FILTER - temporary get all users without current user, later that will work get userwithfriends.
        var query = _context.CustomUsers.Where(u => u.Id != userId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.SearchString))
        {
            query = query.Where(u => u.CustomUserName.Contains(options.SearchString));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    /* THIS IS SIMPLE VERSION BUT ITS REFERENCED somewhere else so i guess i am gonna use the normal one as that has search in it
    public async Task<List<CustomUser>> GetAllAsync()
    {
        // DEFAULT MANDATORY FILTER
        var query = _context.CustomUsers.AsQueryable();

        return await query.ToListAsync();
    }*/

    public async Task<CustomUser?> GetUserWithFriends(int userId)
    {
        return await _context.CustomUsers
            .Include(u => u.Friends)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<CustomUser?> GetByIdAsync(int id)
    {
        return await _context.CustomUsers.FindAsync(id);
    }

    public void Create(CustomUser customUser)
    {
        _context.CustomUsers.Add(customUser);
    }

    public void Delete(CustomUser customUser)
    {
        _context.CustomUsers.Remove(customUser);
    }

    public void Update(CustomUser customUser)
    {
        _context.CustomUsers.Update(customUser);
    }
}
