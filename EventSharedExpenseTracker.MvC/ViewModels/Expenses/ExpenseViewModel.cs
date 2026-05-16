using EventSharedExpenseTracker.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseViewModel
    {
        public int Id { get; set; }
        public int TripId { get; set; }

        public bool CanEdit { get; set; }

        [StringLength(25, ErrorMessage = "The {0} must be at most {1} characters long.")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "The Category field is required.")]
        public string Category { get; set; } = string.Empty;

        [StringLength(80, ErrorMessage = "The {0} must be at most {1} characters long.")]
        public string? Description { get; set; }

        public List<ExpenseParticipantViewModel> Participants { get; set; } = new();

        public List<string> Categories { get; set; } = Expense.Categories;
    }

    public class ExpenseParticipantViewModel
    {
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = "";

        public int? PaidPaymentId { get; set; }   // only needed for Edit
        [Range(0.01, 9999, ErrorMessage = "Value must be positive.")]
        public decimal? PaidAmount { get; set; }

        public int? OwedPaymentId { get; set; }   // only needed for Edit
        public bool IsOwedSelected { get; set; }
        [Range(0, 9999, ErrorMessage = "Value must be positive.")]
        public decimal? OwedAmount { get; set; }
    }
}
