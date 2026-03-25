using Microsoft.AspNetCore.SignalR;
using QuestBoard.Api.Hubs;

namespace QuestBoard.Api.Patterns.Observer;

// [PATTERN: Observer + SignalR] — Subscriber die events broadcast naar alle verbonden clients
// Plugt in de bestaande Observer chain: wanneer een quest wordt voltooid,
// stuurt deze subscriber real-time updates naar de frontend via SignalR.
public class SignalRBroadcasterSubscriber : IQuestEventSubscriber
{
    private readonly IHubContext<QuestBoardHub, IQuestBoardClient> _hubContext;

    public string Name => "SignalRBroadcaster";

    public SignalRBroadcasterSubscriber(IHubContext<QuestBoardHub, IQuestBoardClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public void OnQuestCompleted(QuestCompletedEvent evt)
    {
        var data = new
        {
            QuestTitle = evt.Quest.Title,
            FreelancerName = evt.Freelancer.Name,
            GoldEarned = evt.GoldEarned,
            XpEarned = evt.XpEarned,
            CompletedAt = evt.Quest.CompletedAt
        };

        // Fire-and-forget broadcast naar alle verbonden clients
        _ = _hubContext.Clients.All.QuestCompleted(data);
        _ = _hubContext.Clients.All.LeaderboardUpdated(new
        {
            FreelancerName = evt.Freelancer.Name,
            NewXp = evt.Freelancer.Xp,
            NewGold = evt.Freelancer.Gold,
            NewLevel = evt.Freelancer.Level
        });
    }
}
