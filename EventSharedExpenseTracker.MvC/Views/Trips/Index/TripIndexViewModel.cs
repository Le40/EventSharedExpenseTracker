using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Common;
using EventSharedExpenseTracker.MvC.ViewModels;

namespace EventSharedExpenseTracker.MvC.Views.Trips.Index
{
    public class TripIndexViewModel
    {
        public string? SearchString { get; set; }
        public TripCategory? CategoryFilter { get; set; }
        public bool Creator { get; set; }
        public string? DateSortParam { get; set; }
        public string? CurrentSort { get; set; }

        public string EIdCreateTrip => UiIds.CreateTrip;
        public string EIdTripsCollection => UiIds.TripsCollection;

        public IEnumerable<TripDto> Trips { get; set; } = [];

        public PageControlsViewModel Controls { get; set; } = new();
    }
}
