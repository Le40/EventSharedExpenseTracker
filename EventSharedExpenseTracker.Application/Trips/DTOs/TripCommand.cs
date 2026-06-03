using EventSharedExpenseTracker.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripCommand
    {
        public required string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; }
        public string? ImagePath { get; set; }
        public string BaseCurrencyCode { get; set; } = "EUR";
        public TripCategory Category { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

    }
}
