using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Common;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.MvC.ViewModels.Trips
{
    public class TripIndexViewModel
    {
        public string? SearchString { get; set; }
        public TripCategory? CategoryFilter { get; set; }
        public bool Creator { get; set; }
        public string? DateSortParam { get; set; }
        public string? CurrentSort { get; set; }

        public string EIdCreateTrip => UiIds.CreateTrip;

        public IEnumerable<TripIndexItemViewModel> Trips { get; set; } = [];
    }

    public class TripIndexItemViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string? ImagePath { get; set; }
        public IEnumerable<string> ParticipantNames { get; set; } = [];
    }
}
