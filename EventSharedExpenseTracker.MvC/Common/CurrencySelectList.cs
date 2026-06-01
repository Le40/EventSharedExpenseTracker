using EventSharedExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventSharedExpenseTracker.MvC.Common
{
    public class CurrencySelectList
    {
        public static List<SelectListItem> Get(string selectedCurrencyCode = "EUR")
        {
            return CurrencyMetadata.Currencies
               .OrderBy(c =>
               {
                   var index = Array.IndexOf(
                       PreferredCurrencies,
                       c.Key);

                   return index >= 0 ? index : 999;
               })
               .ThenBy(c => c.Key)
               .Select(c => new SelectListItem
               {
                   Value = c.Key,
                   Text = $"{c.Key} - {c.Value.Name}",
                   Selected = c.Key == selectedCurrencyCode
               })
               .ToList();
        }

        private static readonly string[] PreferredCurrencies =
        {
            "EUR",
            "CZK",
            "USD",
            "GBP",
            "PLN",
            "HUF"
        };
    }
}
