using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Dtos.Mappers
{
    public static class TripMapper
    {
        public static TripDetailsQuery ToDetailsQuery(Trip trip, bool canEdit, int userId)
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
                    .Select(p => new TripParticipantQuery
                    {
                        Id = p.Id,
                        UserName = p.UserName,
                        PaymentSum = p.Payments.Sum(x => x.Ammount),
                        PaymentCount = p.Payments.Count()
                    })
                    .ToList(),

                Expenses = trip.Expenses
                    .Select(e => ExpenseMapper.MapToQuery(e, userId))
                    .ToList()
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
