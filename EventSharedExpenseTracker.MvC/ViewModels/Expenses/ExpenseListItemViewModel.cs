namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";

        public DateTime Date { get; set; }

        public decimal AmountSum { get; set; }

        public bool CanEdit { get; set; }

        public List<PaymentDisplayViewModel> Payments { get; set; } = [];
    }

    public class PaymentDisplayViewModel
    {
        public string ParticipantName { get; set; } = "";

        public decimal Amount { get; set; }

        public bool IsOwed { get; set; }

        public bool IsEquallyShared { get; set; }
    }
}
