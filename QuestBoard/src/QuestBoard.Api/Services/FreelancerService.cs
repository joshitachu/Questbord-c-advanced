using QuestBoard.Api.Data;
using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Patterns.Flyweight;

namespace QuestBoard.Api.Services;

public class FreelancerService : IFreelancerService
{
    private readonly InMemoryDataStore _store;
    private readonly SkillFactory _skillFactory;

    public FreelancerService(InMemoryDataStore store, SkillFactory skillFactory)
    {
        _store = store;
        _skillFactory = skillFactory;
    }

    public FreelancerProfileDto CreateFreelancer(CreateFreelancerDto dto)
    {
        var freelancer = new Freelancer
        {
            Name = dto.Name,
            Email = dto.Email
        };

        _store.Freelancers[freelancer.Id] = freelancer;
        _store.LeaderboardScores[freelancer.Id] = 0;
        return MapToProfile(freelancer);
    }

    public FreelancerProfileDto? GetFreelancer(Guid id)
    {
        return _store.Freelancers.TryGetValue(id, out var freelancer)
            ? MapToProfile(freelancer)
            : null;
    }

    public FreelancerProfileDto? AddSkill(Guid freelancerId, string skillName)
    {
        if (!_store.Freelancers.TryGetValue(freelancerId, out var freelancer)) return null;

        // [PATTERN: Flyweight] — Skill wordt gedeeld via de factory pool
        var skill = _skillFactory.GetSkill(skillName);

        if (!freelancer.Skills.Contains(skill.Name, StringComparer.OrdinalIgnoreCase))
        {
            freelancer.Skills.Add(skill.Name);
        }

        return MapToProfile(freelancer);
    }

    private FreelancerProfileDto MapToProfile(Freelancer f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        Email = f.Email,
        Level = f.Level,
        Xp = f.Xp,
        Gold = f.Gold,
        Skills = f.Skills,
        Badges = f.Badges,
        QuestsCompleted = f.QuestsCompleted,
        AverageRating = f.AverageRating
    };
}
