using QuestBoard.Api.Patterns.Decorator;

namespace QuestBoard.Tests.Patterns;

public class DecoratorTests
{
    [Fact]
    public void BaseQuest_ReturnsUnmodifiedValues()
    {
        // Arrange
        var baseQuest = new BaseQuestBehavior();

        // Act & Assert
        Assert.Equal(100m, baseQuest.CalculateGold(100m));
        Assert.Equal(50, baseQuest.CalculateXp(50));
        Assert.Empty(baseQuest.GetTags());
        Assert.Equal("Slay the dragon", baseQuest.GetDescription("Slay the dragon"));
        Assert.Equal(4, baseQuest.GetMaxTeamSize(4));
    }

    [Fact]
    public void UrgentDecorator_MultipliesGold_By1_5()
    {
        // Arrange
        var quest = new UrgentQuestDecorator(new BaseQuestBehavior());

        // Act
        var gold = quest.CalculateGold(100m);

        // Assert
        Assert.Equal(150m, gold);
    }

    [Fact]
    public void UrgentDecorator_DoublesXp()
    {
        // Arrange
        var quest = new UrgentQuestDecorator(new BaseQuestBehavior());

        // Act
        var xp = quest.CalculateXp(50);

        // Assert
        Assert.Equal(100, xp);
    }

    [Fact]
    public void FeaturedDecorator_AddsXpBonus()
    {
        // Arrange
        var quest = new FeaturedQuestDecorator(new BaseQuestBehavior());

        // Act
        var xp = quest.CalculateXp(50);

        // Assert
        Assert.Equal(100, xp);
    }

    [Fact]
    public void TeamDecorator_MultipliesGoldByTeamSize()
    {
        // Arrange
        var quest = new TeamQuestDecorator(new BaseQuestBehavior(), teamSize: 3);

        // Act
        var gold = quest.CalculateGold(100m);

        // Assert
        Assert.Equal(300m, gold);
    }

    [Fact]
    public void StackedDecorators_UrgentAndFeatured_CombinesEffects()
    {
        // Arrange — wrap base in Urgent, then Featured
        var baseQuest = new BaseQuestBehavior();
        var urgent = new UrgentQuestDecorator(baseQuest);
        var featured = new FeaturedQuestDecorator(urgent);

        // Act
        var gold = featured.CalculateGold(100m);

        // Assert — base * 1.5 (urgent) * 1.2 (featured) = 180
        Assert.Equal(180m, gold);
    }

    [Fact]
    public void StackedDecorators_AllThree_CombinesAllTags()
    {
        // Arrange — wrap base in Urgent, then Featured, then Team
        var baseQuest = new BaseQuestBehavior();
        var urgent = new UrgentQuestDecorator(baseQuest);
        var featured = new FeaturedQuestDecorator(urgent);
        var team = new TeamQuestDecorator(featured, teamSize: 4);

        // Act
        var tags = team.GetTags();

        // Assert
        Assert.Equal(3, tags.Count);
        Assert.Contains("URGENT", tags);
        Assert.Contains("FEATURED", tags);
        Assert.Contains("TEAM", tags);
    }
}
