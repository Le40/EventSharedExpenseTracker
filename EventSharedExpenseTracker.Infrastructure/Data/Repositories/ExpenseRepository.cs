using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            .Include(e => e.Payments)
                .ThenInclude(p=> p.Participant)
            .Where(e => e.TripId == tripId);

        // automatic date sorting - as such sorts were deleted
        query = query.OrderByDescending(e => e.Date);

        if (!string.IsNullOrWhiteSpace(options.SearchString))
        {
            var matchingCategories = Enum.GetValues<ExpenseCategory>()
                .Where(c => c.ToString().Contains(options.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToList();

            query = query.Where(e =>
            e.Name.Contains(options.SearchString) ||
            matchingCategories.Contains(e.Category) ||
            e.Description != null && e.Description.Contains(options.SearchString) ||
            e.Payments.Any(p => p.Participant.DisplayName.Contains(options.SearchString)));
        }

        /*if (options.Category.HasValue)
            query = query.Where(e => e.Category == options.Category.Value);

        if (options.CreatedByMe)
            query = query.Where(x => x.CreatorId == options.UserId);

        query = options.SortBy switch
        {
            "name" => query.OrderBy(e => e.Name),
            "name_desc" => query.OrderByDescending(e => e.Name),
            "amount" => query.OrderBy(e => e.Payments.Where(p => !p.IsOwed).Sum(p => p.AmountBase)),
            "amount_desc" => query.OrderByDescending(e => e.Payments.Where(p => !p.IsOwed).Sum(p => p.AmountBase)),
            "date" => query.OrderBy(e => e.Date),
            _ => query.OrderByDescending(e => e.Date),
        };*/

        return await query
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public async Task<Expense?> GetByOfflineIdAsync(Guid? offlineId)
    {
        return await _context.Expenses
            .FirstOrDefaultAsync(x =>
            x.OfflineClientId == offlineId);
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
