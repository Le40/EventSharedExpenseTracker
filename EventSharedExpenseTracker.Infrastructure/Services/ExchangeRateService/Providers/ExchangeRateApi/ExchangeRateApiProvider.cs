using EventSharedExpenseTracker.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService.Providers.ExchangeRateApi
{
    public class ExchangeRateApiProvider : IExchangeRateApiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ExchangeRateApiProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<Dictionary<string, decimal>> FetchRatesPerEurAsync()
        {
            var apiKey = _configuration["ExchangeRateApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "ExchangeRateApiKey is not configured.");

            var response = await _httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(
                $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/EUR");

            if (response == null)
                throw new InvalidOperationException(
                    "Failed to deserialize exchange rates.");

            if (response.Result != "success")
                throw new InvalidOperationException(
                    $"Exchange Rate API returned '{response.Result}'.");

            var rates = response.ConversionRates;

            return rates;
        }
    }
}