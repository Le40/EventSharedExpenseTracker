using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Trips
{
    public static class TripMapper
    {
        public static TripDetailsQuery ToDetailsQuery(
            Trip trip, 
            bool canEdit,
            IEnumerable<ExpenseQuery> expenses)
        {
            return new TripDetailsQuery
            {
                Id = trip.Id,
                CanEdit = canEdit,
                Name = trip.Name,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                ImagePath = trip.ImagePath,

                Participants = trip.Participants
                    .Select(p => new TripDetailsQueryarticipant
                    {
                        Id = p.Id,
                        IsDummy = p.UserId == null,
                        UserName = p.UserName,
                        PaymentSum = p.Payments.Sum(x => x.Ammount),
                        PaymentCount = p.Payments.Count()
                    })
                    .ToList(),

                Expenses = expenses.ToList()
            };
        }

        public static TripQuery ToQuery(Trip trip)
        {
            return new TripQuery
            {
                Id = trip.Id,
                Name = trip.Name,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                ImagePath = trip.ImagePath,
                ParticipantNames = trip.Participants
                    .Select(p => p.UserName)
                    .ToList()
            };
        }
    }
}
