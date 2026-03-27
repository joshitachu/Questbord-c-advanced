using Microsoft.AspNetCore.Mvc;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Services;

namespace QuestBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestsController : ControllerBase
{
    private readonly IQuestService _questService;

    public QuestsController(IQuestService questService)
    {
        _questService = questService;
    }

    /// <summary>Maak een nieuwe quest aan (Strategy pattern voor pricing)</summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateQuestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = _questService.CreateQuest(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Lijst van alle quests (Decorator pattern voor weergave)</summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_questService.GetAllQuests());
    }

    /// <summary>Quest detail</summary>
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var quest = _questService.GetQuest(id);
        return quest == null ? NotFound() : Ok(quest);
    }

    /// <summary>Freelancer accepteert een quest</summary>
    [HttpPost("{id:guid}/accept")]
    public IActionResult Accept(Guid id, [FromQuery] Guid freelancerId)
    {
        var result = _questService.AcceptQuest(id, freelancerId);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Voltooien van een quest (Observer → XP + Achievements + Leaderboard)</summary>
    [HttpPost("{id:guid}/complete")]
    public IActionResult Complete(Guid id)
    {
        var result = _questService.CompleteQuest(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Matchmaking: vind beste freelancers voor een quest (Strategy pattern)</summary>
    [HttpGet("{id:guid}/matches")]
    public IActionResult GetMatches(Guid id, [FromQuery] string strategy = "SkillBased")
    {
        var matches = _questService.GetMatches(id, strategy);
        return Ok(matches.Select(m => new { m.Freelancer, m.Score }));
    }
}
