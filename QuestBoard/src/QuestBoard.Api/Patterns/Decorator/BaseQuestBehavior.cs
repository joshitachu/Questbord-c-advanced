namespace QuestBoard.Api.Patterns.Decorator;

// [PATTERN: Decorator] Basis quest gedrag zonder modifiers — startpunt voor decorator chain
public class BaseQuestBehavior : IQuestBehavior
{
    public decimal CalculateGold(decimal baseGold) => baseGold;

    public int CalculateXp(int baseXp) => baseXp;

    public List<string> GetTags() => new();

    public string GetDescription(string baseDescription) => baseDescription;

    public int GetMaxTeamSize(int baseSize) => baseSize;
}
