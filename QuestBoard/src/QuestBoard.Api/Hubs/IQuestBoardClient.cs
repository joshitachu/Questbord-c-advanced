namespace QuestBoard.Api.Hubs;

// [SignalR] — Typed hub client interface
// Definieert alle real-time events die de server naar de client kan sturen.
public interface IQuestBoardClient
{
    Task QuestCompleted(object data);
    Task AchievementUnlocked(object data);
    Task LeaderboardUpdated(object data);
    Task QuestAccepted(object data);
}
