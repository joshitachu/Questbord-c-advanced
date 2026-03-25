using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Patterns.Creational;

namespace QuestBoard.Tests.Patterns;

public class SingletonTests
{
    [Fact]
    public void Instance_ReturnsSameReference()
    {
        // Arrange & Act
        var instance1 = GameConfigurationManager.Instance;
        var instance2 = GameConfigurationManager.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_IsThreadSafe()
    {
        // Arrange
        var instances = new GameConfigurationManager[100];

        // Act — parallel access
        Parallel.For(0, 100, i =>
        {
            instances[i] = GameConfigurationManager.Instance;
        });

        // Assert — all references should be the same
        Assert.All(instances, instance => Assert.Same(instances[0], instance));
    }

    [Fact]
    public void CalculateLevel_ReturnsCorrectLevel()
    {
        // Arrange
        var config = GameConfigurationManager.Instance;

        // Act & Assert
        Assert.Equal(1, config.CalculateLevel(0));
        Assert.Equal(1, config.CalculateLevel(999));
        Assert.Equal(2, config.CalculateLevel(1000));
        Assert.Equal(3, config.CalculateLevel(2400));
        Assert.Equal(11, config.CalculateLevel(10000));
    }

    [Fact]
    public void GetBaseXp_ReturnsCorrectValuesPerDifficulty()
    {
        // Arrange
        var config = GameConfigurationManager.Instance;

        // Act & Assert
        Assert.Equal(100, config.GetBaseXp(QuestDifficulty.Easy));
        Assert.Equal(250, config.GetBaseXp(QuestDifficulty.Medium));
        Assert.Equal(500, config.GetBaseXp(QuestDifficulty.Hard));
        Assert.Equal(1000, config.GetBaseXp(QuestDifficulty.Epic));
        Assert.Equal(2000, config.GetBaseXp(QuestDifficulty.Legendary));
    }
}
