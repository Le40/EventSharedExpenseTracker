using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Expenses.DTOs;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Expense>> GetAllFromTripAsync(int tripId, ExpenseQueryOptions options)
    {
        // DEFAULT MANDATORY FILTER
        var query = _context.Expenses
            .Where(e => e.TripId == tripId);

        if (!string.IsNullOrWhiteSpace(options.SearchString))
        {
            query = query.Where(e =>
            e.Name.Contains(options.SearchString) ||
            e.Description != null && e.Description.Contains(options.SearchString));
        }

        if (options.Category.HasValue)
            query = query.Where(e => e.Category == options.Category.Value);

        if (options.CreatedByMe)
            query = query.Where(x => x.CreatorId == options.UserId);

        query = options.SortBy switch
        {
            "name" => query.OrderBy(e => e.Name),
            "name_desc" => query.OrderByDescending(e => e.Name),
            "amount" => query.OrderBy(e => e.Payments.Sum(p => p.AmountBase)),
            "amount_desc" => query.OrderByDescending(e => e.Payments.Sum(p => p.AmountBase)),
            "date" => query.OrderBy(e => e.Date),
            _ => query.OrderByDescending(e => e.Date),
        };

        return await query.AsNoTracking().ToListAsync();
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
