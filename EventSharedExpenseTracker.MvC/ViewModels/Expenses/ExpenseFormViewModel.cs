using EventSharedExpenseTracker.Domain.Constants;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{

    public enum ExpenseFormMode
    {
        Create,
        Edit
    }


    public class ExpenseFormViewModel
    {
        [ValidateNever]
        public string FormId { get; set; } = default!;

        public int Id { get; set; }
        public int TripId { get; set; }
        [StringLength(ExpenseConstants.NameMaxLength)]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required(ErrorMessage = "The Category field is required.")]
        public ExpenseCategory? Category { get; set; }
        //public List<SelectListItem> CategoryOptions { get; set; } = [];
        [StringLength(ExpenseConstants.DescriptionMaxLength)]
        public string? Description { get; set; }

        public string CurrencyCode { get; set; } = "EUR";
        public List<SelectListItem> CurrencyOptions { get; set; } = [];

        public ICollection<ExpenseFormParticipantViewModel> Participants { get; set; } = [];

        // for determining which version of the form to use.
        public ExpenseFormMode Mode { get; set; }
        public string ElementId => Mode == ExpenseFormMode.Create ? UiIds.CreateExpense : UiIds.EditExpense(Id);
        public bool FormIsEdit => Mode == ExpenseFormMode.Edit;
        public bool CanUserEdit { get; set; }
    }

    public class ExpenseFormParticipantViewModel
    {
        public int ParticipantId { get; set; }
        public required string ParticipantName { get; set; }

        public bool IsCurrentUser { get; set; }

        public int? PaidPaymentId { get; set; }   // only needed for Edit
        [Range(0.01, 999999, ErrorMessage = "Value must be positive.")]
        public decimal? PaidAmount { get; set; }

        public int? OwedPaymentId { get; set; }   // only needed for Edit
        public bool IsOwedSelected { get; set; }
        [Range(0, 999999, ErrorMessage = "Value must be positive.")]
        public decimal? OwedAmount { get; set; }
    }
}
