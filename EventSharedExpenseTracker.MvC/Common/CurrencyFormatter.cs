namespace EventSharedExpenseTracker.MvC.Common
{
    public static class CurrencyFormatter
    {
        public static string Format(decimal amount, string currencyCode)
        {
            return currencyCode switch
            {
                "EUR" => $"€{amount:0.00}",
                "USD" => $"${amount:0.00}",
                "GBP" => $"£{amount:0.00}",

                "CZK" => $"{amount:0.00} Kč",
                "PLN" => $"{amount:0.00} zł",
                "HUF" => $"{amount:0.00} Ft",

                _ => $"{amount:0.00} {currencyCode}"
            };
        }
    }
}
