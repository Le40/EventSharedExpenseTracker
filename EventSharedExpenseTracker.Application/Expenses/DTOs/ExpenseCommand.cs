namespace EventSharedExpenseTracker.Application.Expenses.DTOs
{
    public record ExpenseCommand
    {
        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public string Category { get; set; } = "";
        public string? Description { get; set; }

        public List<PaymentCommand> Payments { get; set; } = [];
    }

    public record PaymentCommand
    {
        public int Id { get; set; } // for updates, not needed for creates
        public int ParticipantId { get; set; }
        public decimal? Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }
}

