using EventSharedExpenseTracker.Application.Expenses.DTOs;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripDetailsQuery
    {
        public int Id { get; set; }

        public bool CanEdit { get; set; }

        public string Name { get; set; } = "";

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string? ImagePath { get; set; }

        public List<TripDetailsQueryarticipant> Participants { get; set; } = [];

        public List<ExpenseQuery> Expenses { get; set; } = [];
    }

    public record TripQueryParticipant
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }

    public record TripDetailsQueryarticipant : TripQueryParticipant
    {
        public bool IsDummy { get; set; }
        public decimal PaymentSum { get; set; }

        public int PaymentCount { get; set; }
    }
}
