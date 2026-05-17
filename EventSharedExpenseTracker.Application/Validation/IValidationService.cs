using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Application.Services.Interfaces;

namespace EventSharedExpenseTracker.Application.Validation;

public interface IValidationService
{
    Result<ExpenseCommand> ProcessForSaving(ExpenseCommand command);
    Result ValidateTrip(TripCommand command);
}
