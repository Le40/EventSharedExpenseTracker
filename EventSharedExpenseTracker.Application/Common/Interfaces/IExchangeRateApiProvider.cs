
namespace EventSharedExpenseTracker.Application.Common.Interfaces
{
    public interface IExchangeRateApiProvider
    {
        Task<Dictionary<string, decimal>> FetchRatesPerEurAsync();
    }
}
