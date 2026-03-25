namespace QuestBoard.Api.Patterns.Strategy;

// [PATTERN: Strategy] — Behavioral pattern
// Definieert een familie van algoritmes (pricing), kapselt elk in, en maakt ze uitwisselbaar
public interface IPricingStrategy
{
    string Name { get; }
    decimal CalculatePrice(decimal basePrice, int difficulty, DateTime? deadline);
}
