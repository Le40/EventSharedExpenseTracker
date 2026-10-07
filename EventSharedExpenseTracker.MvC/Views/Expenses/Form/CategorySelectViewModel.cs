using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.MvC.Views.Expenses.Form
{
    public class CategorySelectViewModel
    {
        public string FormId { get; set; } = string.Empty;
        public ExpenseCategory? SelectedCategory { get; set; }
    }
}
