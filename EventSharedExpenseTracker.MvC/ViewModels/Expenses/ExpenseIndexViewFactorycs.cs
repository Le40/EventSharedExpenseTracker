using EventSharedExpenseTracker.Application.Dtos.Mappers;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseIndexViewFactorycs
    {
        public static ExpenseIndexViewModel Create(
            int tripId,
            IEnumerable<Expense> expenses,
            int userId,
            string? sortOrder = null,
            string? searchString = null,
            string? categoryFilter = null,
            bool creator = false)
        {
            var queries = expenses
                .Select(e => ExpenseMapper.MapToQuery(e, userId))
                .ToList();

            return new ExpenseIndexViewModel
            {
                TripId = tripId,

                Expenses = queries
                    .Select(ExpenseFormMapper.FromQuery)
                    .ToList(),

                SearchString = searchString,
                CategoryFilter = categoryFilter,
                Creator = creator,
                CurrentSort = sortOrder,

                NameSortParam = sortOrder == "name" ? "name_desc" : "name",
                DateSortParam = sortOrder == "date" ? "date_desc" : "date",
                AmmSortParam = sortOrder == "amount" ? "amount_desc" : "amount"
            };
        }
    }
}
