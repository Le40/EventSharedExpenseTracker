using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventSharedExpenseTracker.MvC.Common
{
    public class CurrencySelectList
    {
        public static List<SelectListItem> Get(string selectedCurrencyCode = "EUR")
        {
            var currencies = new[]
            {
            ("EUR", "Euro"),
            ("CZK", "Czech koruna"),
            ("USD", "US dollar"),
            ("GBP", "British pound"),
            ("PLN", "Polish złoty"),
            ("HUF", "Hungarian forint")
        };

            return currencies.Select(c => new SelectListItem
            {
                Value = c.Item1,
                Text = $"{c.Item1} - {c.Item2}",
                Selected = c.Item1 == selectedCurrencyCode
            }).ToList();
        }
    }
}
