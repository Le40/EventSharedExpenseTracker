using EventSharedExpenseTracker.MvC.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";
        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; } = DateTime.Today;
        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; } = DateTime.Today;
        public string? ImagePath { get; set; }

        public string BaseCurrencyCode { get; set; } = "EUR";
        public List<SelectListItem> CurrencyOptions { get; set; } = [];

        public TripFormMode Mode { get; set; }
        public string ElementId => Mode == TripFormMode.Create ? UiIds.CreateTrip : UiIds.EditTrip;
        public bool FormIsEdit => Mode == TripFormMode.Edit;
    }
}
