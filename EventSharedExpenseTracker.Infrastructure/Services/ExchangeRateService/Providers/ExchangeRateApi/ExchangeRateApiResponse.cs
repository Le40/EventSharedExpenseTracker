using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService.Providers.ExchangeRateApi
{
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = "";

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = "";

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = [];
    }
}
