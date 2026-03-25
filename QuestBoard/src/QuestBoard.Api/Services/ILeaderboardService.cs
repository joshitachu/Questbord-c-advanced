namespace QuestBoard.Api.Services;

public interface ILeaderboardService
{
    List<object> GetLeaderboard(int top = 10);
}
