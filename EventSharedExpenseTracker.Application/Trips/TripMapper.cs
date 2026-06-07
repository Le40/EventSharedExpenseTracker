using EventSharedExpenseTracker.Application.Common;
using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Trips
{
    public static class TripMapper
    {
        public static TripDetailsQuery ToDetailsQuery(
            Trip trip, 
            bool canUserEdit,
            IEnumerable<ExpenseQuery> expenses)
        {
            return new TripDetailsQuery
            {
                Id = trip.Id,
                CanUserEdit = canUserEdit,
                Name = trip.Name,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                ImagePath = trip.ImagePath,
                BaseCurrencyCode = trip.BaseCurrencyCode,

                Participants = trip.Participants
                    .Select(p => new TripParticipantDetailsQuery
                    {
                        Id = p.Id,
                        IsDummy = p.UserId == null,
                        DisplayName = p.DisplayName,
                        PaymentSum = p.Payments.Sum(x => x.AmountBase),
                        PaymentCount = p.Payments.Count
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
                BaseCurrencyCode = trip.BaseCurrencyCode,
                Country = trip.Country,
                City = trip.City ?? "",
                Category = trip.Category,
                ParticipantNames = trip.Participants
                    .Select(p => p.DisplayName)
                    .ToList()
            };
        }
    }
}
