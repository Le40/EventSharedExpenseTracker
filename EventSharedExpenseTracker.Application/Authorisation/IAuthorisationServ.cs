using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Authorisation;

public interface IAuthorisationServ
{
    bool AuthorisedToView(Trip trip, int userId);
    bool AuthorisedToEdit(Trip trip, int userId);
    bool AuthorisedToEdit(Expense expense, int userId);
}
