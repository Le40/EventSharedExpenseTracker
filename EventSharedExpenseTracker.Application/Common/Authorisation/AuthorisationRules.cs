using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common.Authorisation;

public static class AuthorisationRules
{
    public static bool AuthorisedToView(Trip trip, int userId)
    {
        return trip.HasParticipant(userId);
    }

    public static bool AuthorisedToEdit(Trip trip, int userId)
    {
        return !trip.HasCreator || trip.IsCreatedBy(userId);
    }

    public static bool AuthorisedToEdit(Expense expense, int userId)
    {
        return !expense.HasCreator || expense.IsCreatedBy(userId);
    }
}
