using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml.Linq;

namespace EventSharedExpenseTracker.Infrastructure.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public ExchangeRateService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<decimal> GetRateAsync(
            string fromCurrencyCode,
            string toCurrencyCode)
        {
            fromCurrencyCode = NormalizeCurrencyCode(fromCurrencyCode);
            toCurrencyCode = NormalizeCurrencyCode(toCurrencyCode);

            if (fromCurrencyCode == toCurrencyCode)
                return 1m;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            await EnsureRatesForDateAsync(today);

            var rates = await GetRatesForDateAsync(today);

            return CalculateCrossRate(rates, fromCurrencyCode, toCurrencyCode);
        }

        private static string NormalizeCurrencyCode(string currencyCode)
        {
            return currencyCode.Trim().ToUpperInvariant();
        }

        private async Task EnsureRatesForDateAsync(DateOnly rateDate)
        {
            var hasRates = await _context.ExchangeRates
                .AnyAsync(r => r.RateDate == rateDate);

            if (hasRates)
                return;

            var rates = await FetchRatesFromApiAsync();
            var fetchedAt = DateTime.UtcNow;

            var exchangeRates = rates.Select(r => new ExchangeRate
            {
                CurrencyCode = r.Key,
                RatePerEur = r.Value,
                RateDate = rateDate,
                FetchedAtUtc = fetchedAt
            });

            await _context.ExchangeRates.AddRangeAsync(exchangeRates);
            await _context.SaveChangesAsync();
        }

        private async Task<Dictionary<string, decimal>> FetchRatesFromApiAsync()
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



        private async Task<Dictionary<string, decimal>> GetRatesForDateAsync(DateOnly rateDate)
        {
            return await _context.ExchangeRates
                .Where(r => r.RateDate == rateDate)
                .ToDictionaryAsync(
                    r => r.CurrencyCode,
                    r => r.RatePerEur);
        }

        private static decimal CalculateCrossRate(
            Dictionary<string, decimal> ratesPerEur,
            string fromCurrencyCode,
            string toCurrencyCode)
        {
            if (!ratesPerEur.TryGetValue(fromCurrencyCode, out var fromPerEur))
                throw new InvalidOperationException($"Unsupported currency: {fromCurrencyCode}");

            if (!ratesPerEur.TryGetValue(toCurrencyCode, out var toPerEur))
                throw new InvalidOperationException($"Unsupported currency: {toCurrencyCode}");

            return toPerEur / fromPerEur;
        }
    }
}
