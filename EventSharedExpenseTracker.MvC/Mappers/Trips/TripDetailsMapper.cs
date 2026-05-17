using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Trips;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EventSharedExpenseTracker.MvC.Mappers.Trips
{
    public static class TripDetailsMapper
    {
        public static TripDetailsViewModel FromQuery(TripDetailsQuery query)
        {
            return new TripDetailsViewModel
            {
                Id = query.Id,
                CanUserEdit = query.CanEdit,
                Name = query.Name,
                DateFrom = query.DateFrom,
                DateTo = query.DateTo,
                ImagePath = query.ImagePath,

                TripParticipants = new TripDetailsParticipantsViewModel {
                    TripId = query.Id,
                    CanUserEdit = query.CanEdit,
                    Participants = query.Participants
                        .Select(p => new TripDetailsParticipantViewModel
                        {
                            Id = p.Id,
                            UserName = p.UserName,
                            PaymentSum = p.PaymentSum,
                            PaymentCount = p.PaymentCount
                        })
                        .ToList()
                },

                ExpenseIndex = new ExpenseIndexViewModel
                {
                    TripId = query.Id,
                    Expenses = query.Expenses.Select(ExpenseVMMapper.FromQuery).ToList(),
                    Creator = false,
                    CurrentSort = null,
                    NameSortParam = "name",
                    DateSortParam = "date",
                    AmmSortParam = "amount"
                }
            };
        }
    }
}
