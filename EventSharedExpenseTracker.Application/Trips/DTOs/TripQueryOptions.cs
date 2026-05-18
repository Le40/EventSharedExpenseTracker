using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public class TripQueryOptions
    {
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
        public TripCategory? Category { get; set; }
    }
}
