using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.MvC.ViewModels.Trips
{
    public class TripDetailsViewModel
    {
        public int Id { get; set; }
        public bool CanEdit { get; set; }
        public string Name { get; set; } = "";
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime DateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime DateTo { get; set; }
        public string? ImagePath { get; set; }

        public List<TripDetailsParticipantViewModel> Participants { get; set; } = new();
        public ExpenseIndexViewModel ExpenseIndex { get; set; } = new();

    }

    public class TripDetailsParticipantViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public decimal PaymentSum { get; set; }
        public int PaymentCount { get; set; }
        public bool IsDummy { get; set; } = false;
    }
}
