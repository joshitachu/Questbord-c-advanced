using Microsoft.AspNetCore.Mvc;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Services;

namespace QuestBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>Alle achievements met DSL regels</summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_achievementService.GetAllAchievements());
    }

    /// <summary>Nieuwe achievement met DSL regel (Interpreter pattern)</summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateAchievementDto dto)
    {
        try
        {
            var result = _achievementService.CreateAchievement(dto);
            return Created($"/api/achievements/{result.Id}", result);
        }
        catch (FormatException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>Evalueer achievements voor een freelancer (Interpreter + Flyweight)</summary>
    [HttpPost("evaluate/{freelancerId:guid}")]
    public IActionResult Evaluate(Guid freelancerId)
    {
        var result = _achievementService.EvaluateForFreelancer(freelancerId);
        return Ok(result);
    }
}
