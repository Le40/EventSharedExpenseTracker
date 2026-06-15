
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

                ["ZAR"] = new("South African Rand", "R", true),

                ["ALL"] = new("Albanian Lek", "L", false),
                ["AMD"] = new("Armenian Dram", "֏", true),
                ["ARS"] = new("Argentine Peso", "$", true),
                ["AZN"] = new("Azerbaijani Manat", "₼", true),
                ["BAM"] = new("Bosnia and Herzegovina Convertible Mark", "KM", false),
                ["BRL"] = new("Brazilian Real", "R$", true),

                ["CLP"] = new("Chilean Peso", "$", true),
                ["COP"] = new("Colombian Peso", "$", true),

                ["GEL"] = new("Georgian Lari", "₾", true),

                ["KGS"] = new("Kyrgyzstani Som", "с", false),

                ["LKR"] = new("Sri Lankan Rupee", "Rs", false),
                ["MDL"] = new("Moldovan Leu", "L", false),
                ["MKD"] = new("Macedonian Denar", "ден", false),
                ["MNT"] = new("Mongolian Tögrög", "₮", true),
                ["NPR"] = new("Nepalese Rupee", "Rs", false),
                ["OMR"] = new("Omani Rial", "OMR", false),
                ["PEN"] = new("Peruvian Sol", "S/", false),
                ["QAR"] = new("Qatari Riyal", "QAR", false),
                ["RSD"] = new("Serbian Dinar", "дин", false),
                ["TJS"] = new("Tajikistani Somoni", "ЅМ", false),
                ["UAH"] = new("Ukrainian Hryvnia", "₴", true),
                ["UZS"] = new("Uzbekistani Som", "so'm", false),
            };
    }

    public record CurrencyInfo(
        string Name,
        string Symbol,
        bool SymbolBeforeAmount);
}
