using QuestBoard.Api.Data;

namespace QuestBoard.Api.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly InMemoryDataStore _store;

    public LeaderboardService(InMemoryDataStore store)
    {
        _store = store;
    }

    public List<object> GetLeaderboard(int top = 10)
    {
        return _store.LeaderboardScores
            .OrderByDescending(kv => kv.Value)
            .Take(top)
            .Select((kv, index) =>
            {
                var freelancer = _store.Freelancers.GetValueOrDefault(kv.Key);
                return (object)new
                {
                    Rank = index + 1,
                    FreelancerId = kv.Key,
                    Name = freelancer?.Name ?? "Unknown",
                    Level = freelancer?.Level ?? 0,
                    Score = kv.Value,
                    QuestsCompleted = freelancer?.QuestsCompleted ?? 0,
                    Badges = freelancer?.Badges ?? new List<string>()
                };
            })
            .ToList();
    }
}
