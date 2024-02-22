using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Expense>> GetAllFromTripAsync(int tripId,
        Func<IQueryable<Expense>, IOrderedQueryable<Expense>> orderBy,
        params Func<IQueryable<Expense>, IQueryable<Expense>>[] filters)
    {
        // DEFAULT MANDATORY FILTER
        var query = _context.Expenses.Where(e => e.TripId == tripId);

        foreach (var filter in filters)
        {
            query = filter(query);
        }

        query = orderBy(query);

        return await query.ToListAsync();
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public void Add(Expense expense)
    {
        _context.Expenses.Add(expense);
    }

    public void Update(Expense expense)
    {
        _context.Update(expense);
    }

    public void Delete(Expense expense)
    {
        _context.Remove(expense);
    }
}
