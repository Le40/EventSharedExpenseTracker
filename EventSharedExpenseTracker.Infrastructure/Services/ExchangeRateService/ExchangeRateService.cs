using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService.Providers.ExchangeRateApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateApiProvider _provider;


        public ExchangeRateService(ApplicationDbContext context, IExchangeRateApiProvider provider)
        {
            _context = context;
            _provider = provider;
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

            var rates = await _provider.FetchRatesPerEurAsync();
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
