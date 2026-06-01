using EventSharedExpenseTracker.Application.Common.Interfaces;

namespace EventSharedExpenseTracker.Infrastructure.Services
{
    public class FakeExchangeRateService : IExchangeRateService
    {
        public async Task<decimal> GetRateAsync(
            string fromCurrencyCode,
            string toCurrencyCode)
        {
            if (fromCurrencyCode == toCurrencyCode)
                return 1m;

            var ratesToEur = new Dictionary<string, decimal>
            {
                ["EUR"] = 1m,
                ["CZK"] = 0.040m,
                ["USD"] = 0.92m,
                ["GBP"] = 1.17m
            };

            var fromToEur = ratesToEur[fromCurrencyCode];
            var toToEur = ratesToEur[toCurrencyCode];

            return fromToEur / toToEur;
        }
    }
}
