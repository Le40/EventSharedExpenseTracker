namespace EventSharedExpenseTracker.Application.Dtos
{
    public class ExpenseCommand
    {

        public int Id { get; set; }
        public int TripId { get; set; }
        public bool CanEdit { get; set; }

        public string Name { get; set; } = "";
        public DateTime Date { get; set; }
        public string Category { get; set; } = "";
        public string? Description { get; set; }

        public List<PaymentCommand> Payments { get; set; } = [];
    }

    public class PaymentCommand
    {
        public int Id { get; set; } // for updates, not needed for creates
        public int ParticipantId { get; set; }
        public decimal? Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }
}

