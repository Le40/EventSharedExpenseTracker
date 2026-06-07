using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Trips;

namespace EventSharedExpenseTracker.MvC.Mappers.Trips
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
                //BaseCurrencyCode = query.BaseCurrencyCode,

                TripParticipants = new TripDetailsParticipantsViewModel {
                    TripId = query.Id,
                    CanUserEdit = query.CanUserEdit,
                    BaseCurrencyCode = query.BaseCurrencyCode,
                    Participants = query.Participants
                        .Select(p => new TripDetailsParticipantViewModel
                        {
                            Id = p.Id,
                            IsDummy = p.IsDummy,
                            UserName = p.DisplayName,
                            PaymentSum = p.PaymentSum,
                            PaymentCount = p.PaymentCount
                        })
                        .ToList()
                },

                ExpenseIndex = new ExpenseIndexViewModel
                {
                    TripId = query.Id,
                    Expenses = query.Expenses.Select(e => ExpenseVMMapper.FromQuery(e, query.BaseCurrencyCode)).ToList(),
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
