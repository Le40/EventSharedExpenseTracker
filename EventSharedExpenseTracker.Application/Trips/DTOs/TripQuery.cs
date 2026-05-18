using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripQuery
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; }
        public string? ImagePath { get; set; }

        public List<string> ParticipantNames { get; set; } = [];
    }
}
