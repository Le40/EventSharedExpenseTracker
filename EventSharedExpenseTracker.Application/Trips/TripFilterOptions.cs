using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Application.Trips
{
    public class TripFilterOptions
    {
        public string? SearchString { get; set; }
        public string? SortBy { get; set; }
        public TripCategory? Category { get; set; }
    }
}
