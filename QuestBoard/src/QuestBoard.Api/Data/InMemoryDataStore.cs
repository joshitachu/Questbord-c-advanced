using System.Collections.Concurrent;
using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Api.Data;

// [DATA STORE] Centrale in-memory opslag — geen database nodig, focus op design patterns
public class InMemoryDataStore
{
    public ConcurrentDictionary<Guid, Quest> Quests { get; } = new();
    public ConcurrentDictionary<Guid, Freelancer> Freelancers { get; } = new();
    public ConcurrentDictionary<Guid, Client> Clients { get; } = new();
    public ConcurrentDictionary<Guid, Achievement> Achievements { get; } = new();

    // Leaderboard: freelancer ID → total score
    public ConcurrentDictionary<Guid, int> LeaderboardScores { get; } = new();

    // Notification log for demo purposes
    public ConcurrentBag<string> NotificationLog { get; } = new();
}
