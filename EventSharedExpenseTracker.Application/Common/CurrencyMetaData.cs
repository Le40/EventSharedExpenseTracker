using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Application.Common
{
    public static class CurrencyMetadata
    {
        public static readonly Dictionary<string, CurrencyInfo> Currencies =
            new()
            {
                ["EUR"] = new("Euro", "€", true),
                ["USD"] = new("US Dollar", "$", true),
                ["GBP"] = new("British Pound", "£", true),
                ["CHF"] = new("Swiss Franc", "CHF", false),

                ["CZK"] = new("Czech Koruna", "Kč", false),
                ["PLN"] = new("Polish Złoty", "zł", false),
                ["HUF"] = new("Hungarian Forint", "Ft", false),
                ["RON"] = new("Romanian Leu", "lei", false),
                ["BGN"] = new("Bulgarian Lev", "лв", false),
                ["DKK"] = new("Danish Krone", "kr", false),
                ["SEK"] = new("Swedish Krona", "kr", false),
                ["NOK"] = new("Norwegian Krone", "kr", false),
                ["ISK"] = new("Icelandic Króna", "kr", false),

                ["CAD"] = new("Canadian Dollar", "C$", true),
                ["MXN"] = new("Mexican Peso", "Mex$", true),

                ["TRY"] = new("Turkish Lira", "₺", true),

                ["MAD"] = new("Moroccan Dirham", "MAD", false),
                ["EGP"] = new("Egyptian Pound", "E£", true),
                ["TND"] = new("Tunisian Dinar", "TND", false),

                ["AED"] = new("UAE Dirham", "AED", false),
                ["SAR"] = new("Saudi Riyal", "SAR", false),
                ["JOD"] = new("Jordanian Dinar", "JOD", false),
                ["ILS"] = new("Israeli Shekel", "₪", true),

                ["INR"] = new("Indian Rupee", "₹", true),
                ["THB"] = new("Thai Baht", "฿", true),
                ["VND"] = new("Vietnamese Đồng", "₫", true),
                ["IDR"] = new("Indonesian Rupiah", "Rp", true),
                ["MYR"] = new("Malaysian Ringgit", "RM", true),
                ["SGD"] = new("Singapore Dollar", "S$", true),
                ["PHP"] = new("Philippine Peso", "₱", true),

                ["JPY"] = new("Japanese Yen", "¥", true),
                ["KRW"] = new("South Korean Won", "₩", true),
                ["CNY"] = new("Chinese Yuan", "¥", true),
                ["HKD"] = new("Hong Kong Dollar", "HK$", true),

                ["AUD"] = new("Australian Dollar", "A$", true),
                ["NZD"] = new("New Zealand Dollar", "NZ$", true),

                ["ZAR"] = new("South African Rand", "R", true)
            };
    }

    public record CurrencyInfo(
        string Name,
        string Symbol,
        bool SymbolBeforeAmount);
}
