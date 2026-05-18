namespace EventSharedExpenseTracker.Application.Expenses.DTOs
{
    public record ExpenseQuery
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public bool CanEdit { get; set; }

        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public string Category { get; set; } = "";
        public string? Description { get; set; }
        public List<PaymentQuery> Payments { get; set; } = [];
    }

    public record PaymentQuery
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = "";
        public decimal Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }
}

