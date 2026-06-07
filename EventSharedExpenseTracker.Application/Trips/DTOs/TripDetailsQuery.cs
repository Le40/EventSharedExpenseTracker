using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripDetailsQuery
    {
        public int Id { get; set; }

        public bool CanUserEdit { get; set; }

        public required string Name { get; set; }

        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }

        public string? ImagePath { get; set; }

        public string BaseCurrencyCode { get; set; } = "EUR";
        public TripCategory Category { get; set; }
        public string Country { get; set; } = "";
        public string City { get; set; } = "";

        public ICollection<TripParticipantDetailsQuery> Participants { get; set; } = [];

        public ICollection<ExpenseQuery> Expenses { get; set; } = [];
    }
}
