using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.MvC.Views.Expenses.Index;

namespace EventSharedExpenseTracker.MvC.Views.Trips.Details
{
    public static class TripDetailsMapper
    {
        public static TripDetailsViewModel FromQuery(TripDetailsQuery query)
        {
            return new TripDetailsViewModel
            {
                Id = query.Id,
                CanUserEdit = query.CanUserEdit,
                Name = query.Name,
                DateFrom = query.DateFrom,
                DateTo = query.DateTo,
                ImagePath = query.ImagePath,
                Category = query.Category,
                //BaseCurrencyCode = query.BaseCurrencyCode,
                Statistics = query.Statistics,

                TripParticipants = new TripDetailsParticipantsViewModel {
                    TripId = query.Id,
                    CanUserEdit = query.CanUserEdit,
                    BaseCurrencyCode = query.BaseCurrencyCode,
                    Participants = query.Participants
                        .Select(p => new TripParticipantDto
                        {
                            Id = p.Id,
                            CanBeDeleted = p.CanBeDeleted,
                            IsDummy = p.IsDummy,
                            DisplayName = p.DisplayName,
                            PaymentSum = p.PaymentSum,
                            PaymentCount = p.PaymentCount
                        })
                        .ToList()
                },

                ExpenseIndex = new ExpenseIndexViewModel
                {
                    TripId = query.Id,
                    Expenses = query.Expenses.Select(e => ExpenseIndexMapper.FromQuery(e, query.BaseCurrencyCode)).ToList(),
                    Creator = false,
                    CurrentSort = null,
                    NameSortParam = "name",
                    DateSortParam = "date",
                    AmountSortParam = "amount",

                    BaseCurrencyCode = query.BaseCurrencyCode
                }
            };
        }
    }
}
