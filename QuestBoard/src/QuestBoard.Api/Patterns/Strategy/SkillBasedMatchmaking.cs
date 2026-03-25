namespace QuestBoard.Api.Patterns.Strategy;

using QuestBoard.Api.Models.Domain;

// [PATTERN: Strategy] — Concrete strategy: scores freelancers by percentage of required skills they possess
public class SkillBasedMatchmaking : IMatchmakingStrategy
{
    public string Name => "SkillBased";

    public List<(Freelancer Freelancer, double Score)> FindMatches(Quest quest, IEnumerable<Freelancer> freelancers, int maxResults = 5)
    {
        if (quest.RequiredSkills.Count == 0)
        {
            // No required skills — everyone scores equally
            return freelancers
                .Select(f => (Freelancer: f, Score: 1.0))
                .Take(maxResults)
                .ToList();
        }

        return freelancers
            .Select(f =>
            {
                var matchingSkills = f.Skills
                    .Count(skill => quest.RequiredSkills.Contains(skill, StringComparer.OrdinalIgnoreCase));
                var score = (double)matchingSkills / quest.RequiredSkills.Count;
                return (Freelancer: f, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .ToList();
    }
}
