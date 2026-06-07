using EventSharedExpenseTracker.Domain.Enums;
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
        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateOnly DateFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateOnly DateTo { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public string? ImagePath { get; set; }
        [Required(ErrorMessage = "The Category field is required.")]
        public TripCategory? Category { get; set; }
        //public List<SelectListItem> CategoryOptions { get; set; } = [];

        [Display(Name = "Currency")]
        public string BaseCurrencyCode { get; set; } = "EUR";
        public List<SelectListItem> CurrencyOptions { get; set; } = [];
        public string Country { get; set; } = "";
        public List<SelectListItem> CountryOptions { get; set; } = [];
        public string? City { get; set; }

        public TripFormMode Mode { get; set; }
        public string ElementId => Mode == TripFormMode.Create ? UiIds.CreateTrip : UiIds.EditTrip;
        public bool FormIsEdit => Mode == TripFormMode.Edit;
    }
}
