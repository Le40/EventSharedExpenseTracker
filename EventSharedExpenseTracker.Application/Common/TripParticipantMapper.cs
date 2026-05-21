using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common
{
    public static class TripParticipantMapper
    {
        public static string GetDisplayName(TripParticipant participant)
        {
            return participant.User?.CustomUserName
                ?? participant.DisplayName;
        }
    }
}
