using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }

        public string? ImagePath { get; set; }
        public string BaseCurrencyCode { get; set; } = "EUR";
        public TripCategory Category { get; set; }

        public string? Country { get; set; }
        public string? City { get; set; }

        public ICollection<TripParticipantDto> Participants { get; set; } = [];
    }
}
