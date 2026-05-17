using EventSharedExpenseTracker.MvC.Common;
using System.ComponentModel.DataAnnotations;
using static EventSharedExpenseTracker.MvC.ViewModels.Expenses.ExpenseFormViewModel;

namespace EventSharedExpenseTracker.MvC.ViewModels.Trips
{
    public enum TripFormMode
    {
        Create,
        Edit
    }

    public class TripFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TripFormMode Mode { get; set; }
        public string ElementId => Mode == TripFormMode.Create ? UiIds.CreateTrip : UiIds.EditTrip;
        public bool FormIsEdit => Mode == TripFormMode.Edit;


        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; } = DateTime.Today;
        //public bool CanEdit { get; set; }
        public string? ImagePath { get; set; }
    }
}
