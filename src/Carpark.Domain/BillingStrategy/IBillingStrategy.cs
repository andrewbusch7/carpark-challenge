namespace Carpark.Domain.BillingStrategy
{
    public interface IBillingStrategy
    {
        string Name { get; }
        bool IsApplicable();
        decimal CalculateCost();
    }
}
