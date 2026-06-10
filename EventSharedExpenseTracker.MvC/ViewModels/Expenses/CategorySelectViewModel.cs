using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class CategorySelectViewModel
    {
        public string FormId { get; set; } = string.Empty;
        public ExpenseCategory? SelectedCategory { get; set; }
    }
}
