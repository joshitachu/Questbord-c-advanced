using QuestBoard.Api.Data;
using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Patterns.Flyweight;
using QuestBoard.Api.Patterns.Observer;

namespace QuestBoard.Tests.Patterns;

public class ObserverTests
{
    // Simple test subscriber that tracks whether OnQuestCompleted was called
    private class TestSubscriber : IQuestEventSubscriber
    {
        public string Name => "TestSubscriber";
        public bool WasCalled { get; private set; }

        public void OnQuestCompleted(QuestCompletedEvent evt)
        {
            WasCalled = true;
        }
    }

    private static Quest CreateTestQuest() => new()
    {
        Title = "Slay the Dragon",
        Description = "Defeat the ancient dragon in the northern mountains",
        BaseGold = 500m,
        BaseXp = 200
    };

    private static Freelancer CreateTestFreelancer() => new()
    {
        Name = "TestHero",
        Email = "hero@questboard.com"
    };

    [Fact]
    public void Publisher_Subscribe_AddsSubscriber()
    {
        // Arrange
        var publisher = new QuestEventPublisher();
        var subscriber = new TestSubscriber();

        // Act
        publisher.Subscribe(subscriber);

        // Assert
        Assert.Single(publisher.GetSubscribers());
        Assert.Same(subscriber, publisher.GetSubscribers()[0]);
    }

    [Fact]
    public void Publisher_Notify_CallsAllSubscribers()
    {
        // Arrange
        var publisher = new QuestEventPublisher();
        var subscriber1 = new TestSubscriber();
        var subscriber2 = new TestSubscriber();
        publisher.Subscribe(subscriber1);
        publisher.Subscribe(subscriber2);

        var quest = CreateTestQuest();
        var freelancer = CreateTestFreelancer();
        var evt = new QuestCompletedEvent(quest, freelancer, 500m, 200);

        // Act
        publisher.Notify(evt);

        // Assert
        Assert.True(subscriber1.WasCalled, "First subscriber should have been called.");
        Assert.True(subscriber2.WasCalled, "Second subscriber should have been called.");
    }

    [Fact]
    public void Publisher_Unsubscribe_RemovesSubscriber()
    {
        // Arrange
        var publisher = new QuestEventPublisher();
        var subscriber = new TestSubscriber();
        publisher.Subscribe(subscriber);

        // Act
        publisher.Unsubscribe(subscriber);

        // Assert
        Assert.Empty(publisher.GetSubscribers());
    }

    [Fact]
    public void XpCalculator_AddsXpAndGold()
    {
        // Arrange
        var subscriber = new XpCalculatorSubscriber();
        var quest = CreateTestQuest();
        var freelancer = CreateTestFreelancer();
        var evt = new QuestCompletedEvent(quest, freelancer, 500m, 200);

        // Act
        subscriber.OnQuestCompleted(evt);

        // Assert
        Assert.Equal(200, freelancer.Xp);
        Assert.Equal(500m, freelancer.Gold);
        Assert.Equal(1, freelancer.QuestsCompleted);
    }

    [Fact]
    public void XpCalculator_CalculatesLevel()
    {
        // Arrange — give enough XP for level 3 (needs 2000+ XP total → level = 1 + 2000/1000 = 3)
        var subscriber = new XpCalculatorSubscriber();
        var quest = CreateTestQuest();
        var freelancer = CreateTestFreelancer();

        // Fire two events of 1200 XP each → total 2400 XP → level = 1 + (2400/1000) = 1 + 2 = 3
        var evt1 = new QuestCompletedEvent(quest, freelancer, 100m, 1200);
        var evt2 = new QuestCompletedEvent(quest, freelancer, 100m, 1200);

        // Act
        subscriber.OnQuestCompleted(evt1);
        subscriber.OnQuestCompleted(evt2);

        // Assert
        Assert.Equal(2400, freelancer.Xp);
        Assert.Equal(3, freelancer.Level);
        Assert.Equal(2, freelancer.QuestsCompleted);
    }

    [Fact]
    public void LeaderboardUpdater_UpdatesScore()
    {
        // Arrange
        var dataStore = new InMemoryDataStore();
        var subscriber = new LeaderboardUpdaterSubscriber(dataStore);
        var quest = CreateTestQuest();
        var freelancer = CreateTestFreelancer();
        freelancer.Xp = 500;
        freelancer.Gold = 250m;

        var evt = new QuestCompletedEvent(quest, freelancer, 0m, 0);

        // Act
        subscriber.OnQuestCompleted(evt);

        // Assert — score = Xp + (int)Gold = 500 + 250 = 750
        Assert.True(dataStore.LeaderboardScores.ContainsKey(freelancer.Id));
        Assert.Equal(750, dataStore.LeaderboardScores[freelancer.Id]);
    }
}
