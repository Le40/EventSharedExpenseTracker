using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common.Authorisation;

public interface IAuthorisationService
{
    bool AuthorisedToView(Trip trip, int userId);
    bool AuthorisedToEdit(Trip trip, int userId);
    bool AuthorisedToEdit(Expense expense, int userId);
}
