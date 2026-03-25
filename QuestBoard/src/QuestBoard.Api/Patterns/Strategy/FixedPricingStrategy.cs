namespace QuestBoard.Api.Patterns.Strategy;

// [PATTERN: Strategy] — Concrete strategy: returns the base price unchanged
public class FixedPricingStrategy : IPricingStrategy
{
    public string Name => "Fixed";

    public decimal CalculatePrice(decimal basePrice, int difficulty, DateTime? deadline)
    {
        return basePrice;
    }
}
