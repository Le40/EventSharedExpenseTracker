namespace EventSharedExpenseTracker.Application.Expenses.Queries
{
    public record TripExpensesQuery
    {
        public int TripId { get; set; }
        public string BaseCurrencyCode { get; set; } = "EUR";
        public ICollection<ExpenseQuery> Expenses { get; set; } = [];
    }
}
