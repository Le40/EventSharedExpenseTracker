using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Common;
using EventSharedExpenseTracker.MvC.ViewModels;
using EventSharedExpenseTracker.MvC.Views.Expenses.Index;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.MvC.Views.Trips.Details
{
    public class TripDetailsViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public required string Name { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateOnly DateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateOnly DateTo { get; set; }
        public string? ImagePath { get; set; }

        public TripCategory Category { get; set; }

        public string BaseCurrencyCode { get; set; } = "EUR";

        public TripDetailsParticipantsViewModel TripParticipants { get; set; } = new();
        public ExpenseIndexViewModel ExpenseIndex { get; set; } = new();

        public bool CanUserEdit { get; set; }

        public TripStatistics Statistics { get; set; } = default!;
        public string EIdExpensesCollection => UiIds.ExpenseCollection;
        public string EIdTripParticipants => UiIds.TripParticipants;
        public string EIdSearchParticipants => UiIds.SearchParticipants;
        public string EIdEditTrip => UiIds.EditTrip;

        public PageControlsViewModel Controls { get; set; } = new PageControlsViewModel();

    }

    public class TripDetailsParticipantsViewModel
    {
        public int TripId {  get; set; }
        public bool CanUserEdit { get; set; }
        public string EIdTripParticipants => UiIds.TripParticipants;
        public string BaseCurrencyCode { get; set; } = "EUR";

        public IEnumerable<TripParticipantDto> Participants { get; set; } = [];
    }

}
