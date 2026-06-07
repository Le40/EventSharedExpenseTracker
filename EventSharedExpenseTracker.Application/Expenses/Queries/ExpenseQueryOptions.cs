using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Application.Expenses.Queries
{
    public class ExpenseQueryOptions
    {
        public int UserId { get; set; }
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
        public bool CreatedByMe { get; set; }
        public ExpenseCategory? Category { get; set; }
    }
}
