using EventSharedExpenseTracker.Application.Common;
using EventSharedExpenseTracker.Domain.ValueObjects;

namespace EventSharedExpenseTracker.MvC.Common
{
    public static class CurrencyFormatter
    {
        public static string Format(decimal amount, string currencyCode)
        {
            if (!CurrencyMetadata.Currencies.TryGetValue(
            currencyCode,
            out var currency))
            {
                return $"{amount:0.00} {currencyCode}";
            }

            return currency.SymbolBeforeAmount
                ? $"{currency.Symbol}{amount:0.00}"
                : $"{amount:0.00} {currency.Symbol}";
        }

        public static string FormatMoney(Money money)
        {
            if (!CurrencyMetadata.Currencies.TryGetValue(
            money.CurrencyCode,
            out var currency))
            {
                return $"{money.Amount:0.00} {money.CurrencyCode}";
            }

            return currency.SymbolBeforeAmount
                ? $"{currency.Symbol}{money.Amount:0.00}"
                : $"{money.Amount:0.00} {currency.Symbol}";
        }
    }
}
