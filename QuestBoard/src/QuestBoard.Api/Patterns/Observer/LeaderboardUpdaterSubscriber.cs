using QuestBoard.Api.Data;

namespace QuestBoard.Api.Patterns.Observer;

// [PATTERN: Observer] — Concrete subscriber
// Werkt de leaderboard score bij na een quest completion
public class LeaderboardUpdaterSubscriber : IQuestEventSubscriber
{
    private readonly InMemoryDataStore _dataStore;

    public string Name => "LeaderboardUpdater";

    public LeaderboardUpdaterSubscriber(InMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public void OnQuestCompleted(QuestCompletedEvent evt)
    {
        var freelancer = evt.Freelancer;
        _dataStore.LeaderboardScores[freelancer.Id] = freelancer.Xp + (int)(freelancer.Gold);
    }
}
