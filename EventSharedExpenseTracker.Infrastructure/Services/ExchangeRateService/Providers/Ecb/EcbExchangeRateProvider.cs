using EventSharedExpenseTracker.Application.Common.Interfaces;
using System.Globalization;
using System.Xml.Linq;

namespace EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService.Providers.Ecb
{
    public class EcbExchangeRateProvider : IExchangeRateApiProvider
    {
        private readonly HttpClient _httpClient;

        public EcbExchangeRateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<string, decimal>> FetchRatesPerEurAsync()
        {
            var xml = await _httpClient.GetStringAsync(
                "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");

            var doc = XDocument.Parse(xml);

            var rates = new Dictionary<string, decimal>
            {
                ["EUR"] = 1m
            };

            foreach (var cube in doc.Descendants().Where(x => x.Attribute("currency") != null))
            {
                var currency = cube.Attribute("currency")!.Value;
                var rate = decimal.Parse(
                    cube.Attribute("rate")!.Value,
                    CultureInfo.InvariantCulture);

                rates[currency] = rate;
            }

            return rates;
        }
    }
}
