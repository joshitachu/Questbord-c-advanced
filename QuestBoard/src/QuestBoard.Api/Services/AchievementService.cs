using QuestBoard.Api.Data;
using QuestBoard.Api.Models.Domain;
using QuestBoard.Api.Models.DTOs;
using QuestBoard.Api.Patterns.Flyweight;
using QuestBoard.Api.Patterns.Interpreter;

namespace QuestBoard.Api.Services;

public class AchievementService : IAchievementService
{
    private readonly InMemoryDataStore _store;
    private readonly BadgeFactory _badgeFactory;

    public AchievementService(InMemoryDataStore store, BadgeFactory badgeFactory)
    {
        _store = store;
        _badgeFactory = badgeFactory;
    }

    public List<Achievement> GetAllAchievements()
    {
        return _store.Achievements.Values.ToList();
    }

    public Achievement CreateAchievement(CreateAchievementDto dto)
    {
        // Validate DSL rule parses correctly
        AchievementRuleParser.Parse(dto.DslRule);

        var achievement = new Achievement
        {
            Name = dto.Name,
            Description = dto.Description,
            BadgeName = dto.BadgeName,
            DslRule = dto.DslRule,
            XpReward = dto.XpReward,
            GoldReward = dto.GoldReward
        };

        _store.Achievements[achievement.Id] = achievement;
        return achievement;
    }

    public object EvaluateForFreelancer(Guid freelancerId)
    {
        if (!_store.Freelancers.TryGetValue(freelancerId, out var freelancer))
            return new { Error = "Freelancer not found" };

        // [PATTERN: Interpreter] — Bouw context van freelancer stats en evalueer DSL regels
        var context = new AchievementContext(new Dictionary<string, double>
        {
            ["quests.completed"] = freelancer.QuestsCompleted,
            ["rating.avg"] = freelancer.AverageRating,
            ["level"] = freelancer.Level,
            ["gold"] = (double)freelancer.Gold,
            ["xp"] = freelancer.Xp,
            ["skills.count"] = freelancer.Skills.Count,
            ["badges.count"] = freelancer.Badges.Count
        });

        var results = new List<object>();

        foreach (var achievement in _store.Achievements.Values)
        {
            var expression = AchievementRuleParser.Parse(achievement.DslRule);
            var earned = expression.Interpret(context);
            var alreadyHas = freelancer.Badges.Contains(achievement.BadgeName);

            if (earned && !alreadyHas)
            {
                // [PATTERN: Flyweight] — Badge wordt gedeeld via factory
                _badgeFactory.GetBadge(achievement.BadgeName, achievement.Description, "Gold");
                freelancer.Badges.Add(achievement.BadgeName);
                freelancer.Xp += achievement.XpReward;
                freelancer.Gold += achievement.GoldReward;
            }

            results.Add(new
            {
                Achievement = achievement.Name,
                Rule = achievement.DslRule,
                Earned = earned,
                AlreadyHad = alreadyHas,
                NewlyAwarded = earned && !alreadyHas
            });
        }

        return new
        {
            FreelancerId = freelancer.Id,
            FreelancerName = freelancer.Name,
            Results = results
        };
    }
}
