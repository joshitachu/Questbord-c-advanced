namespace QuestBoard.Api.Patterns.Strategy;

// [PATTERN: Strategy] — Concrete strategy: adjusts price based on difficulty and deadline urgency
public class DynamicPricingStrategy : IPricingStrategy
{
    public string Name => "Dynamic";

    // Difficulty multipliers mapped by QuestDifficulty enum value (int cast):
    // Easy=0 -> 0.8, Medium=1 -> 1.0, Hard=2 -> 1.3, Epic=3 -> 1.6, Legendary=4 -> 2.0
    private static readonly Dictionary<int, decimal> DifficultyMultipliers = new()
    {
        { 0, 0.8m },   // Easy
        { 1, 1.0m },   // Medium
        { 2, 1.3m },   // Hard
        { 3, 1.6m },   // Epic
        { 4, 2.0m }    // Legendary
    };

    private const decimal UrgencySurcharge = 1.25m;
    private const int UrgencyThresholdDays = 3;

    public decimal CalculatePrice(decimal basePrice, int difficulty, DateTime? deadline)
    {
        var multiplier = DifficultyMultipliers.GetValueOrDefault(difficulty, 1.0m);
        var price = basePrice * multiplier;

        // If deadline is within 3 days, add 25% urgency surcharge
        if (deadline.HasValue && (deadline.Value - DateTime.UtcNow).TotalDays <= UrgencyThresholdDays)
        {
            price *= UrgencySurcharge;
        }

        return price;
    }
}
