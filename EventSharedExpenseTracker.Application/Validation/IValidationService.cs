using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Application.Validation;

public class AppValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<AppValidationError> Errors { get; } = new();

    public void AddError(string message, string? propertyName = null)
    {
        Errors.Add(new AppValidationError(propertyName, message));
    }
}

public record AppValidationError(string? PropertyName, string Message);

public interface IValidationService
{
    //AppValidationResult ValidateExpense(Expense expense);
    Result<ExpenseCommand> ProcessForSaving(ExpenseCommand command);
    Result ValidateTrip(TripCommand command);
    //AppValidationResult ValidateTrip(Trip trip);
}
