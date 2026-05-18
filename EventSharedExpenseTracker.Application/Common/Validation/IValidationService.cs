using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Trips.DTOs;

namespace EventSharedExpenseTracker.Application.Common.Validation;

public interface IValidationService
{
    Result<ExpenseCommand> ProcessForSaving(ExpenseCommand command);
    Result ValidateTrip(TripCommand command);
}
