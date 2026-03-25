using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Api.Patterns.Creational;

// [PATTERN: Singleton] — Creational pattern
// Thread-safe Lazy<T> singleton die alle game configuratie centraliseert.
// Voorkomt magic numbers verspreid over meerdere klassen.
public sealed class GameConfigurationManager
{
    private static readonly Lazy<GameConfigurationManager> _instance =
        new(() => new GameConfigurationManager(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static GameConfigurationManager Instance => _instance.Value;

    private GameConfigurationManager()
    {
        BaseXpByDifficulty = new Dictionary<QuestDifficulty, int>
        {
            { QuestDifficulty.Easy, 100 },
            { QuestDifficulty.Medium, 250 },
            { QuestDifficulty.Hard, 500 },
            { QuestDifficulty.Epic, 1000 },
            { QuestDifficulty.Legendary, 2000 }
        };

        DifficultyPriceMultipliers = new Dictionary<QuestDifficulty, decimal>
        {
            { QuestDifficulty.Easy, 0.8m },
            { QuestDifficulty.Medium, 1.0m },
            { QuestDifficulty.Hard, 1.3m },
            { QuestDifficulty.Epic, 1.6m },
            { QuestDifficulty.Legendary, 2.0m }
        };
    }

    // === XP & Leveling ===
    public int XpPerLevel { get; } = 1000;

    public int CalculateLevel(int totalXp) => 1 + (totalXp / 1000);

    // === Base XP per difficulty ===
    public IReadOnlyDictionary<QuestDifficulty, int> BaseXpByDifficulty { get; }

    public int GetBaseXp(QuestDifficulty difficulty) =>
        BaseXpByDifficulty.TryGetValue(difficulty, out var xp) ? xp : 250;

    // === Pricing ===
    public IReadOnlyDictionary<QuestDifficulty, decimal> DifficultyPriceMultipliers { get; }
    public decimal UrgencySurcharge { get; } = 1.25m;
    public int UrgencyThresholdDays { get; } = 3;

    // === Decorator Multipliers ===
    public decimal UrgentGoldMultiplier { get; } = 1.5m;
    public int UrgentXpMultiplier { get; } = 2;
    public decimal FeaturedGoldMultiplier { get; } = 1.2m;
    public int FeaturedBonusXp { get; } = 50;
    public decimal TeamXpMultiplier { get; } = 0.8m;

    // === Team Constraints ===
    public int MinTeamSize { get; } = 2;
    public int DefaultMaxTeamSize { get; } = 1;
}
