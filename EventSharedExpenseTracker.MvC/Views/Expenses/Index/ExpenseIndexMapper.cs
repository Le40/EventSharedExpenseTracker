using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Domain.ValueObjects;

namespace EventSharedExpenseTracker.MvC.Views.Expenses.Index
{
    public class ExpenseIndexMapper
    {
        public static ExpenseListItemViewModel FromQuery(ExpenseQuery query, string tripCurrencyCode)
        {
            var paidPayments = query.Payments.Where(p => !p.IsOwed).ToList();
            var owedPayments = query.Payments.Where(p => p.IsOwed).ToList();


            return new ExpenseListItemViewModel
            {
                Id = query.Id,
                Name = query.Name,
                Category = query.Category,
                Date = query.Date,
                CanUserEdit = query.CanUserEdit,
                PaidPayments = paidPayments,
                OwedPayments = owedPayments,
                TotalPaidBase = new Money(paidPayments.Sum(p => p.AmountBase), tripCurrencyCode),
                TotalPaidOriginal = new Money(paidPayments.Sum(p => p.AmountOriginal), query.CurrencyCode),
                CurrencyCode = tripCurrencyCode
            };
        }
    }
}
