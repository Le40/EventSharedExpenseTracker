using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Validation;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}


public interface IValidationService
{
    ValidationResult ValidateExpense(Expense expense);
    ValidationResult ValidateTrip(Trip trip);
}
