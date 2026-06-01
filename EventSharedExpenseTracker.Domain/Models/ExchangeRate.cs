
namespace EventSharedExpenseTracker.Domain.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; } = "";
        public decimal RatePerEur { get; set; }
        public DateOnly RateDate { get; set; }
        public DateTime FetchedAtUtc {  get; set; }
    }
}
