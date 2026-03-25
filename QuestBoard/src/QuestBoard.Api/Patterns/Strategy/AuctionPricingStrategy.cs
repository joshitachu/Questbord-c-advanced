namespace QuestBoard.Api.Patterns.Strategy;

// [PATTERN: Strategy] — Concrete strategy: starts at 80% of base price (auction discount)
public class AuctionPricingStrategy : IPricingStrategy
{
    public string Name => "Auction";

    private const decimal AuctionDiscount = 0.80m;

    public decimal CalculatePrice(decimal basePrice, int difficulty, DateTime? deadline)
    {
        return basePrice * AuctionDiscount;
    }
}
