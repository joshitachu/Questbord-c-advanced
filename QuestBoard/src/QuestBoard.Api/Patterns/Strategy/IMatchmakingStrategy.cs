namespace QuestBoard.Api.Patterns.Strategy;

using QuestBoard.Api.Models.Domain;

// [PATTERN: Strategy] — Behavioral pattern
// Definieert een familie van algoritmes (matchmaking), kapselt elk in, en maakt ze uitwisselbaar
public interface IMatchmakingStrategy
{
    string Name { get; }
    List<(Freelancer Freelancer, double Score)> FindMatches(Quest quest, IEnumerable<Freelancer> freelancers, int maxResults = 5);
}
