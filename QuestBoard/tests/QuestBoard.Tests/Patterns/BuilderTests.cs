using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Patterns.Creational;

namespace QuestBoard.Tests.Patterns;

public class BuilderTests
{
    private static readonly Guid TestClientId = Guid.NewGuid();

    [Fact]
    public void Build_WithRequiredFields_CreatesQuest()
    {
        // Arrange & Act
        var quest = QuestBuilder.Create()
            .WithTitle("Test Quest")
            .ForClient(TestClientId)
            .Build();

        // Assert
        Assert.Equal("Test Quest", quest.Title);
        Assert.Equal(TestClientId, quest.ClientId);
        Assert.Equal(QuestStatus.Open, quest.Status);
    }

    [Fact]
    public void Build_FluentChaining_SetsAllProperties()
    {
        // Arrange & Act
        var quest = QuestBuilder.Create()
            .WithTitle("Build API")
            .WithDescription("Develop REST API")
            .ForClient(TestClientId)
            .WithDifficulty(QuestDifficulty.Hard)
            .WithType(QuestType.Development)
            .WithPricing(PricingType.Dynamic, 500m)
            .WithSkills("C#", "ASP.NET")
            .AsUrgent()
            .Build();

        // Assert
        Assert.Equal("Build API", quest.Title);
        Assert.Equal("Develop REST API", quest.Description);
        Assert.Equal(QuestDifficulty.Hard, quest.Difficulty);
        Assert.Equal(QuestType.Development, quest.Type);
        Assert.Equal(PricingType.Dynamic, quest.PricingType);
        Assert.Equal(500m, quest.BaseGold);
        Assert.True(quest.IsUrgent);
        Assert.Contains("C#", quest.RequiredSkills);
        Assert.Contains("ASP.NET", quest.RequiredSkills);
    }

    [Fact]
    public void Build_WithoutBaseXp_UsesDefaultFromSingleton()
    {
        // Arrange & Act
        var quest = QuestBuilder.Create()
            .WithTitle("Hard Quest")
            .ForClient(TestClientId)
            .WithDifficulty(QuestDifficulty.Hard)
            .Build();

        // Assert — Hard difficulty = 500 XP from GameConfigurationManager
        Assert.Equal(500, quest.BaseXp);
    }

    [Fact]
    public void Build_WithoutTitle_ThrowsInvalidOperationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            QuestBuilder.Create()
                .ForClient(TestClientId)
                .Build());
    }

    [Fact]
    public void Build_WithoutClientId_ThrowsInvalidOperationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            QuestBuilder.Create()
                .WithTitle("No Client Quest")
                .Build());
    }

    [Fact]
    public void Build_AsTeamQuest_ClampsMinTeamSize()
    {
        // Arrange & Act — MaxTeamSize 1 should be clamped to MinTeamSize (2)
        var quest = QuestBuilder.Create()
            .WithTitle("Team Quest")
            .ForClient(TestClientId)
            .AsTeamQuest(1)
            .Build();

        // Assert
        Assert.True(quest.IsTeamQuest);
        Assert.Equal(2, quest.MaxTeamSize); // clamped from 1 to MinTeamSize=2
    }
}
