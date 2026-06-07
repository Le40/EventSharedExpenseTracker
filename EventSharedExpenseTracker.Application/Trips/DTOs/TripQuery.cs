using EventSharedExpenseTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripQuery
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        [DataType(DataType.Date)]
        public DateOnly DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateOnly DateTo { get; set; }
        public string? ImagePath { get; set; }
        public string BaseCurrencyCode { get; set; } = "EUR";
        public TripCategory Category { get; set; }
        public string Country { get; set; } = "";
        public string City { get; set; } = "";

        public ICollection<string> ParticipantNames { get; set; } = [];
    }
}
