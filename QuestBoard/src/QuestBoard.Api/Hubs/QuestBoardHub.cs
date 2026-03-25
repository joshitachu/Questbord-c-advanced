using Microsoft.AspNetCore.SignalR;

namespace QuestBoard.Api.Hubs;

// [SignalR] — Hub voor real-time communicatie
// Clients verbinden via /hub/questboard en ontvangen events
// zoals QuestCompleted, AchievementUnlocked, en LeaderboardUpdated.
public class QuestBoardHub : Hub<IQuestBoardClient>
{
    private readonly ILogger<QuestBoardHub> _logger;

    public QuestBoardHub(ILogger<QuestBoardHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client verbonden: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client verbroken: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
