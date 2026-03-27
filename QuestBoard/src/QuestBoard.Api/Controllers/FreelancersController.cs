using Microsoft.AspNetCore.Mvc;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Services;

namespace QuestBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FreelancersController : ControllerBase
{
    private readonly IFreelancerService _freelancerService;

    public FreelancersController(IFreelancerService freelancerService)
    {
        _freelancerService = freelancerService;
    }

    /// <summary>Registreer een nieuwe freelancer</summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateFreelancerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = _freelancerService.CreateFreelancer(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Freelancer profiel ophalen (Flyweight skills/badges)</summary>
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var result = _freelancerService.GetFreelancer(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Skill toevoegen aan freelancer (Flyweight pattern)</summary>
    [HttpPost("{id:guid}/skills")]
    public IActionResult AddSkill(Guid id, [FromBody] AddSkillDto dto)
    {
        var result = _freelancerService.AddSkill(id, dto.SkillName);
        return result == null ? NotFound() : Ok(result);
    }
}
