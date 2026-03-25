using Microsoft.AspNetCore.Mvc;
using QuestBoard.Api.Services;

namespace QuestBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    /// <summary>Top freelancers leaderboard</summary>
    [HttpGet]
    public IActionResult Get([FromQuery] int top = 10)
    {
        return Ok(_leaderboardService.GetLeaderboard(top));
    }
}
