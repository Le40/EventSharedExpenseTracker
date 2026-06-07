using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Application.Expenses.Queries
{
    public record ExpenseQuery
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public bool CanUserEdit { get; set; }

        public required string Name { get; set; }
        public DateOnly Date { get; set; }
        public ExpenseCategory Category { get; set; }
        public string? Description { get; set; }
        public string CurrencyCode { get; set; } = "EUR";
        public ICollection<PaymentQuery> Payments { get; set; } = [];
    }
}

