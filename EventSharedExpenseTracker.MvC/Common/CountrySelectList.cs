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
            ["AL"] = "Albania",
            ["AM"] = "Armenia",
            ["AR"] = "Argentina",
            ["AT"] = "Austria",
            ["AU"] = "Australia",
            ["AZ"] = "Azerbaijan",
            ["BA"] = "Bosnia and Herzegovina",
            ["BE"] = "Belgium",
            ["BG"] = "Bulgaria",
            ["BR"] = "Brazil",
            ["CA"] = "Canada",
            ["CH"] = "Switzerland",
            ["CL"] = "Chile",
            ["CN"] = "China",
            ["CO"] = "Colombia",
            ["CZ"] = "Czech Republic",
            ["DE"] = "Germany",
            ["DK"] = "Denmark",
            ["EE"] = "Estonia",
            ["EG"] = "Egypt",
            ["ES"] = "Spain",
            ["FI"] = "Finland",
            ["FR"] = "France",
            ["GB"] = "United Kingdom",
            ["GE"] = "Georgia",
            ["GR"] = "Greece",
            ["HK"] = "Hong Kong",
            ["HR"] = "Croatia",
            ["HU"] = "Hungary",
            ["ID"] = "Indonesia",
            ["IE"] = "Ireland",
            ["IL"] = "Israel",
            ["IN"] = "India",
            ["IS"] = "Iceland",
            ["IT"] = "Italy",
            ["JO"] = "Jordan",
            ["JP"] = "Japan",
            ["KG"] = "Kyrgyzstan",
            ["KR"] = "South Korea",
            ["KZ"] = "Kazakhstan",
            ["LK"] = "Sri Lanka",
            ["LT"] = "Lithuania",
            ["LU"] = "Luxembourg",
            ["LV"] = "Latvia",
            ["MA"] = "Morocco",
            ["MD"] = "Moldova",
            ["ME"] = "Montenegro",
            ["MK"] = "North Macedonia",
            ["MN"] = "Mongolia",
            ["MX"] = "Mexico",
            ["MY"] = "Malaysia",
            ["NL"] = "Netherlands",
            ["NO"] = "Norway",
            ["NP"] = "Nepal",
            ["NZ"] = "New Zealand",
            ["OM"] = "Oman",
            ["PE"] = "Peru",
            ["PH"] = "Philippines",
            ["PL"] = "Poland",
            ["PT"] = "Portugal",
            ["QA"] = "Qatar",
            ["RO"] = "Romania",
            ["RS"] = "Serbia",
            ["SA"] = "Saudi Arabia",
            ["SE"] = "Sweden",
            ["SG"] = "Singapore",
            ["SI"] = "Slovenia",
            ["SK"] = "Slovakia",
            ["TH"] = "Thailand",
            ["TJ"] = "Tajikistan",
            ["TN"] = "Tunisia",
            ["TR"] = "Turkey",
            ["UA"] = "Ukraine",
            ["US"] = "United States",
            ["UZ"] = "Uzbekistan",
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
