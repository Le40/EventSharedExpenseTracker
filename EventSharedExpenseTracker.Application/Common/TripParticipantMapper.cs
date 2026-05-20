using EventSharedExpenseTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
