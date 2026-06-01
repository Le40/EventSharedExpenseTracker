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
                ["JPY"] = new("Japanese Yen", "¥", true),
                ["BGN"] = new("Bulgarian Lev", "лв", false),
                ["CZK"] = new("Czech Koruna", "Kč", false),
                ["DKK"] = new("Danish Krone", "kr", false),
                ["GBP"] = new("British Pound", "£", true),
                ["HUF"] = new("Hungarian Forint", "Ft", false),
                ["PLN"] = new("Polish Złoty", "zł", false),
                ["RON"] = new("Romanian Leu", "lei", false),
                ["SEK"] = new("Swedish Krona", "kr", false),
                ["CHF"] = new("Swiss Franc", "CHF", false),
                ["ISK"] = new("Icelandic Króna", "kr", false),
                ["NOK"] = new("Norwegian Krone", "kr", false),
                ["TRY"] = new("Turkish Lira", "₺", true),
                ["AUD"] = new("Australian Dollar", "A$", true),
                ["BRL"] = new("Brazilian Real", "R$", true),
                ["CAD"] = new("Canadian Dollar", "C$", true),
                ["CNY"] = new("Chinese Yuan", "¥", true),
                ["HKD"] = new("Hong Kong Dollar", "HK$", true),
                ["IDR"] = new("Indonesian Rupiah", "Rp", true),
                ["ILS"] = new("Israeli Shekel", "₪", true),
                ["INR"] = new("Indian Rupee", "₹", true),
                ["KRW"] = new("South Korean Won", "₩", true),
                ["MXN"] = new("Mexican Peso", "$", true),
                ["MYR"] = new("Malaysian Ringgit", "RM", true),
                ["NZD"] = new("New Zealand Dollar", "NZ$", true),
                ["PHP"] = new("Philippine Peso", "₱", true),
                ["SGD"] = new("Singapore Dollar", "S$", true),
                ["THB"] = new("Thai Baht", "฿", true),
                ["ZAR"] = new("South African Rand", "R", true)
            };
    }

    public record CurrencyInfo(
        string Name,
        string Symbol,
        bool SymbolBeforeAmount);
}
