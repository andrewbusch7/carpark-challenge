namespace Carpark.Domain
{
    public class Billing
    {
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public string BillingStrategyName { get; set; }

        public Billing(decimal cost, string currency, string billingStrategyName)
        {
            Cost = cost;
            Currency = currency;
            BillingStrategyName = billingStrategyName;
        }
    }
}
