
namespace EventSharedExpenseTracker.Domain.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public string FromCurrency { get; set; } = "";
        public string ToCurrency { get; set; } = "EUR";
        public decimal Rate { get; set; }
        public DateOnly RateDate { get; set; }
        public DateTime FetchedAtUtc {  get; set; }
    }
}
