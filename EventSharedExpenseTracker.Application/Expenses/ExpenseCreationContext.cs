
namespace EventSharedExpenseTracker.Application.Expenses
{
    public class ExpenseCreationContext
    {
        public int TripId { get; init; }
        public int UserId { get; init; }
        public string TripBaseCurrencyCode { get; init; } = "EUR";
        public decimal ExchangeRateToBase { get; init; }
    }
}
