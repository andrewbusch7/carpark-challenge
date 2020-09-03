using Carpark.Domain.BillingStrategy;

namespace Carpark.Domain
{
    public class Billing
    {
        public decimal Cost { get; set; }
        public const string Currency = "AUD";
        public string RateName { get; set; }
        public string RateType { get; set; }

        // For tests
        public Billing() { }

        public Billing(IBillingStrategy billingStrategy)
        {
            Cost = billingStrategy.CalculateCost();
            RateName = billingStrategy.Name;
            RateType = billingStrategy.Type;
        }
    }
}
