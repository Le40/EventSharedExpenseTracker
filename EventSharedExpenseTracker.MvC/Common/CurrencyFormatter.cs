using EventSharedExpenseTracker.Application.Common;
using EventSharedExpenseTracker.Domain.ValueObjects;

namespace EventSharedExpenseTracker.MvC.Common
{
    public static class CurrencyFormatter
    {
        public static string Format(decimal amount, string currencyCode, bool showDecimals = true)
        {
            var numberFormat = showDecimals
                ? "#,##0.00"
                : "#,##0";

            var formattedAmount = amount.ToString(numberFormat);

            if (!CurrencyMetadata.Currencies.TryGetValue(
                    currencyCode,
                    out var currency))
            {
                return $"{formattedAmount} {currencyCode}";
            }

            return currency.SymbolBeforeAmount
                ? $"{currency.Symbol}{formattedAmount}"
                : $"{formattedAmount} {currency.Symbol}";
        }

        public static string FormatMoney(Money money, bool showDecimals = true)
        {
            return Format(
                money.Amount,
                money.CurrencyCode,
                showDecimals);
        }
    }
}
