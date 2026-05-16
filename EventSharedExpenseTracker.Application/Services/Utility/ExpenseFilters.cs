using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Utility;

public static class ExpenseFilters
{
    public static Func<IQueryable<Expense>, IQueryable<Expense>> Search(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return query => query;

        return query => query.Where(e =>
            e.Name.ToLower().Contains(searchString.ToLower()) ||
            e.Category.ToLower().Contains(searchString.ToLower()) ||
            e.Description != null && e.Description.ToLower().Contains(searchString.ToLower()));
    }

    public static Func<IQueryable<Expense>, IQueryable<Expense>> CategoryFilter(string categoryFilter)
    {
        if (string.IsNullOrWhiteSpace(categoryFilter))
            return query => query;

        return query => query.Where(e => e.Category == categoryFilter);
    }

    public static Func<IQueryable<Expense>, IQueryable<Expense>> CreatorFilter(bool creator, int userId)
    {
        /*if (!creator)
            return query => query;

        return query => query.Where(e => e.CreatorId == creatorId);*/

        if (creator)
        {
            return query => query.Where(e => e.CreatorId == userId);
        }
        return query => query;
    }

    public static Func<IQueryable<Expense>, IOrderedQueryable<Expense>> GetOrderByExpression(string sortOrder)
    {
        switch (sortOrder)
        {
            case "name":
                return q => q.OrderBy(e => e.Name);
            case "name_desc":
                return q => q.OrderByDescending(e => e.Name);
            case "date":
                return q => q.OrderBy(e => e.Date);
            case "date_desc":
                return q => q.OrderByDescending(e => e.Date);
            case "amount":
                return q => q.OrderBy(e => e.AmountSum);
            case "amount_desc":
                return q => q.OrderByDescending(e => e.AmountSum);
            default:
                return q => q.OrderByDescending(e => e.Id);
        }
    }
}
