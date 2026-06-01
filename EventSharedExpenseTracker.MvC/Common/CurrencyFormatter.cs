using EventSharedExpenseTracker.Application.Common;

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
    }
}
