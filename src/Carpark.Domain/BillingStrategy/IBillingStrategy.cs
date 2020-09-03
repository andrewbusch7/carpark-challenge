namespace Carpark.Domain.BillingStrategy
{
    public interface IBillingStrategy
    {
        string Name { get; }
        string Type { get; }
        bool IsApplicable();
        decimal CalculateCost();
    }
}
