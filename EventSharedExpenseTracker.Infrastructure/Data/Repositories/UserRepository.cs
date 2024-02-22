using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomUser>> GetAllAsync(int userId,
        params Func<IQueryable<CustomUser>, IQueryable<CustomUser>>[] filters)
    {
        // DEFAULT MANDATORY FILTER
        var query = _context.CustomUsers.AsQueryable();

        foreach (var filter in filters)
        {
            query = filter(query);
        }

        return await query.ToListAsync();
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
