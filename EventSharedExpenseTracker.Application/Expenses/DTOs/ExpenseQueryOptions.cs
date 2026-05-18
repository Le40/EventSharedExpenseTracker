
namespace EventSharedExpenseTracker.Application.Expenses.DTOs
{
    public class ExpenseQueryOptions
    {
        public int UserId { get; set; }
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
        public bool CreatedByMe { get; set; }
        public string? Category { get; set; }
    }
}
