using EventSharedExpenseTracker.Application.Expenses.DTOs;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripDetailsQuery
    {
        public int Id { get; set; }

        public bool CanUserEdit { get; set; }

        public required string Name { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string? ImagePath { get; set; }

        public ICollection<TripDetailsQueryarticipant> Participants { get; set; } = [];

        public ICollection<ExpenseQuery> Expenses { get; set; } = [];
    }

    public record TripQueryParticipant
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
    }

    public record TripDetailsQueryarticipant : TripQueryParticipant
    {
        public bool IsDummy { get; set; }
        public decimal PaymentSum { get; set; }

        public int PaymentCount { get; set; }
    }
}
