using QuestBoard.Api.Patterns.Strategy;
using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Tests.Patterns;

// [PATTERN: Strategy] — Tests for pricing and matchmaking strategy implementations
public class StrategyTests
{
    // ── Pricing Strategy Tests ──────────────────────────────────────────

    [Fact]
    public void FixedPricing_ReturnsBasePrice()
    {
        // Arrange
        var strategy = new FixedPricingStrategy();
        var basePrice = 100m;

        // Act
        var result = strategy.CalculatePrice(basePrice, (int)QuestDifficulty.Hard, DateTime.UtcNow.AddDays(10));

        // Assert
        Assert.Equal(basePrice, result);
    }

    [Fact]
    public void AuctionPricing_Returns80Percent()
    {
        // Arrange
        var strategy = new AuctionPricingStrategy();
        var basePrice = 200m;

        // Act
        var result = strategy.CalculatePrice(basePrice, (int)QuestDifficulty.Medium, null);

        // Assert
        Assert.Equal(160m, result); // 200 * 0.80
    }

    [Fact]
    public void DynamicPricing_AdjustsForDifficulty()
    {
        // Arrange
        var strategy = new DynamicPricingStrategy();
        var basePrice = 100m;

        // Act — Hard difficulty (enum value 2) should apply 1.3x multiplier
        var result = strategy.CalculatePrice(basePrice, (int)QuestDifficulty.Hard, DateTime.UtcNow.AddDays(30));

        // Assert
        Assert.Equal(130m, result); // 100 * 1.3
    }

    [Fact]
    public void DynamicPricing_AddsUrgencySurcharge()
    {
        // Arrange
        var strategy = new DynamicPricingStrategy();
        var basePrice = 100m;
        var urgentDeadline = DateTime.UtcNow.AddDays(1); // Within 3 days

        // Act — Medium difficulty (1.0x) + urgency surcharge (1.25x)
        var result = strategy.CalculatePrice(basePrice, (int)QuestDifficulty.Medium, urgentDeadline);

        // Assert
        Assert.Equal(125m, result); // 100 * 1.0 * 1.25
    }

    // ── Matchmaking Strategy Tests ──────────────────────────────────────

    [Fact]
    public void SkillBasedMatchmaking_RanksbySkillOverlap()
    {
        // Arrange
        var strategy = new SkillBasedMatchmaking();
        var quest = new Quest
        {
            Title = "Build API",
            RequiredSkills = new List<string> { "C#", "ASP.NET", "SQL" }
        };

        var freelancers = new List<Freelancer>
        {
            new() { Name = "Alice", Skills = new List<string> { "C#", "ASP.NET", "SQL" } },      // 3/3 = 1.0
            new() { Name = "Bob",   Skills = new List<string> { "C#" } },                         // 1/3 = 0.33
            new() { Name = "Carol", Skills = new List<string> { "C#", "ASP.NET" } },              // 2/3 = 0.67
            new() { Name = "Dave",  Skills = new List<string> { "Python", "Django" } }             // 0/3 = 0.0
        };

        // Act
        var results = strategy.FindMatches(quest, freelancers);

        // Assert — Should be ranked: Alice, Carol, Bob, Dave
        Assert.Equal("Alice", results[0].Freelancer.Name);
        Assert.Equal(1.0, results[0].Score, precision: 2);
        Assert.Equal("Carol", results[1].Freelancer.Name);
        Assert.Equal("Bob", results[2].Freelancer.Name);
        Assert.Equal("Dave", results[3].Freelancer.Name);
        Assert.Equal(0.0, results[3].Score, precision: 2);
    }

    [Fact]
    public void RatingBasedMatchmaking_RanksByRating()
    {
        // Arrange
        var strategy = new RatingBasedMatchmaking();
        var quest = new Quest
        {
            Title = "Design Logo",
            RequiredSkills = new List<string> { "Design" }
        };

        var freelancers = new List<Freelancer>
        {
            new() { Name = "Alice", AverageRating = 4.5, Skills = new List<string> { "Design" } },    // rating: 0.72 + skill: 0.2 = 0.92
            new() { Name = "Bob",   AverageRating = 5.0, Skills = new List<string> { "C#" } },         // rating: 0.80 + skill: 0.0 = 0.80
            new() { Name = "Carol", AverageRating = 3.0, Skills = new List<string> { "Design" } },     // rating: 0.48 + skill: 0.2 = 0.68
            new() { Name = "Dave",  AverageRating = 2.0, Skills = new List<string> { "Python" } }      // rating: 0.32 + skill: 0.0 = 0.32
        };

        // Act
        var results = strategy.FindMatches(quest, freelancers);

        // Assert — Should be ranked: Alice (0.92), Bob (0.80), Carol (0.68), Dave (0.32)
        Assert.Equal("Alice", results[0].Freelancer.Name);
        Assert.Equal(0.92, results[0].Score, precision: 2);
        Assert.Equal("Bob", results[1].Freelancer.Name);
        Assert.Equal(0.80, results[1].Score, precision: 2);
        Assert.Equal("Carol", results[2].Freelancer.Name);
        Assert.Equal("Dave", results[3].Freelancer.Name);
    }
}
