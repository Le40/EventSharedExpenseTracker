using EventSharedExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventSharedExpenseTracker.MvC.Common
{
    public class CurrencySelectList
    {
        public static List<SelectListItem> Get(string selectedCurrencyCode = "EUR")
        {
            var currencies = CurrencyMetadata.Currencies;

            return currencies.Select(c => new SelectListItem
            {
                Value = c.Key,
                Text = $"{c.Key} - {c.Value.Name}",
                Selected = c.Key == selectedCurrencyCode
            }).ToList();
        }
    }
}
