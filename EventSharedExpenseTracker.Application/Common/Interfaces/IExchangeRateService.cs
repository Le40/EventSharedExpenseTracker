
namespace EventSharedExpenseTracker.Application.Common.Interfaces
{
    public interface IExchangeRateService
    {
        Task<decimal> GetRateAsync(string currencyCode, string fromCurrencyCode);
    }
}
