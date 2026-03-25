namespace QuestBoard.Api.Patterns.Strategy;

using QuestBoard.Api.Models.Domain;

// [PATTERN: Strategy] — Concrete strategy: scores by AverageRating (normalized 0-1) with a small bonus for skill overlap
public class RatingBasedMatchmaking : IMatchmakingStrategy
{
    public string Name => "RatingBased";

    private const double RatingWeight = 0.8;
    private const double SkillBonusWeight = 0.2;
    private const double MaxRating = 5.0;

    public List<(Freelancer Freelancer, double Score)> FindMatches(Quest quest, IEnumerable<Freelancer> freelancers, int maxResults = 5)
    {
        return freelancers
            .Select(f =>
            {
                // Normalize rating from 0-5 scale to 0-1
                var ratingScore = f.AverageRating / MaxRating;

                // Small bonus for skill overlap
                var skillBonus = 0.0;
                if (quest.RequiredSkills.Count > 0)
                {
                    var matchingSkills = f.Skills
                        .Count(skill => quest.RequiredSkills.Contains(skill, StringComparer.OrdinalIgnoreCase));
                    skillBonus = (double)matchingSkills / quest.RequiredSkills.Count;
                }

                var score = (ratingScore * RatingWeight) + (skillBonus * SkillBonusWeight);
                return (Freelancer: f, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .ToList();
    }
}
