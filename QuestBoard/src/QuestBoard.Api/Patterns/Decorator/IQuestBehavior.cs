namespace QuestBoard.Api.Patterns.Decorator;

// [PATTERN: Decorator] — Structural pattern
// Voegt dynamisch extra verantwoordelijkheden toe aan quest objecten
public interface IQuestBehavior
{
    decimal CalculateGold(decimal baseGold);
    int CalculateXp(int baseXp);
    List<string> GetTags();
    string GetDescription(string baseDescription);
    int GetMaxTeamSize(int baseSize);
}
