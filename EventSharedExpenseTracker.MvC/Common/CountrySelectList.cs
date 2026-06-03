using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventSharedExpenseTracker.MvC.Common
{
    public static class CountrySelectList
    {
        public static List<SelectListItem> Get(string? selected = null)
        {
            var countries = Countries;

            return countries
                .OrderBy(c => c.Value)
                .Select(c => new SelectListItem
                {
                    Value = c.Key,
                    Text = c.Value,
                    Selected = c.Key == selected
                })
                .ToList();
        }

        public static readonly Dictionary<string, string> Countries = new()
            {
                ["AT"] = "Austria",
                ["AU"] = "Australia",
                ["BG"] = "Bulgaria",
                ["CA"] = "Canada",
                ["CH"] = "Switzerland",
                ["CN"] = "China",
                ["CZ"] = "Czech Republic",
                ["DE"] = "Germany",
                ["DK"] = "Denmark",
                ["EG"] = "Egypt",
                ["ES"] = "Spain",
                ["FR"] = "France",
                ["GB"] = "United Kingdom",
                ["HK"] = "Hong Kong",
                ["HU"] = "Hungary",
                ["ID"] = "Indonesia",
                ["IE"] = "Ireland",
                ["IL"] = "Israel",
                ["IN"] = "India",
                ["IS"] = "Iceland",
                ["IT"] = "Italy",
                ["JO"] = "Jordan",
                ["JP"] = "Japan",
                ["KR"] = "South Korea",
                ["MA"] = "Morocco",
                ["MX"] = "Mexico",
                ["MY"] = "Malaysia",
                ["NL"] = "Netherlands",
                ["NO"] = "Norway",
                ["NZ"] = "New Zealand",
                ["PH"] = "Philippines",
                ["PL"] = "Poland",
                ["RO"] = "Romania",
                ["SA"] = "Saudi Arabia",
                ["SE"] = "Sweden",
                ["SG"] = "Singapore",
                ["SK"] = "Slovakia",
                ["TH"] = "Thailand",
                ["TN"] = "Tunisia",
                ["TR"] = "Turkey",
                ["US"] = "United States",
                ["VN"] = "Vietnam",
                ["ZA"] = "South Africa"
            };

        public static readonly Dictionary<string, string> DefaultCurrencies =
    new()
    {
        ["AT"] = "EUR",
        ["AU"] = "AUD",
        ["BG"] = "BGN",
        ["CA"] = "CAD",
        ["CH"] = "CHF",
        ["CN"] = "CNY",
        ["CZ"] = "CZK",
        ["DE"] = "EUR",
        ["DK"] = "DKK",
        ["EG"] = "EGP",
        ["ES"] = "EUR",
        ["FR"] = "EUR",
        ["GB"] = "GBP",
        ["HK"] = "HKD",
        ["HU"] = "HUF",
        ["ID"] = "IDR",
        ["IE"] = "EUR",
        ["IL"] = "ILS",
        ["IN"] = "INR",
        ["IS"] = "ISK",
        ["IT"] = "EUR",
        ["JO"] = "JOD",
        ["JP"] = "JPY",
        ["KR"] = "KRW",
        ["MA"] = "MAD",
        ["MX"] = "MXN",
        ["MY"] = "MYR",
        ["NL"] = "EUR",
        ["NO"] = "NOK",
        ["NZ"] = "NZD",
        ["PH"] = "PHP",
        ["PL"] = "PLN",
        ["RO"] = "RON",
        ["SA"] = "SAR",
        ["SE"] = "SEK",
        ["SG"] = "SGD",
        ["SK"] = "EUR",
        ["TH"] = "THB",
        ["TN"] = "TND",
        ["TR"] = "TRY",
        ["US"] = "USD",
        ["VN"] = "VND",
        ["ZA"] = "ZAR"
    };
    }
}
