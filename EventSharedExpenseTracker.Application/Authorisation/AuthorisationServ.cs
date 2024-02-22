using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Authorisation;

public class AuthorisationServ : IAuthorisationServ
{
    public bool AuthorisedToView(Trip trip, int userId)
    {
        return trip != null && trip.Participants.Any(p => p.UserId == userId);
    }

    public bool AuthorisedToEdit(Trip trip, int userId)
    {
        return trip != null && (trip.CreatorId == userId);
    }

    public bool AuthorisedToEdit(Expense expense, int userId)
    {
        return expense != null && (expense.CreatorId == userId || expense.CreatorId == null);
    }
}
