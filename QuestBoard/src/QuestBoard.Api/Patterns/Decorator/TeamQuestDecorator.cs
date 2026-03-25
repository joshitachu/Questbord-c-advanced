namespace QuestBoard.Api.Patterns.Decorator;

// [PATTERN: Decorator] Team modifier: gold * teamSize, XP penalty per lid
public class TeamQuestDecorator : QuestDecorator
{
    private readonly int _teamSize;

    public TeamQuestDecorator(IQuestBehavior inner, int teamSize) : base(inner)
    {
        _teamSize = teamSize;
    }

    public override decimal CalculateGold(decimal baseGold) =>
        _inner.CalculateGold(baseGold) * _teamSize;

    public override int CalculateXp(int baseXp) =>
        (int)(_inner.CalculateXp(baseXp) * 0.8);

    public override List<string> GetTags()
    {
        var tags = _inner.GetTags();
        tags.Add("TEAM");
        return tags;
    }

    public override string GetDescription(string baseDescription) =>
        _inner.GetDescription(baseDescription) + $" [Team: max {_teamSize} members]";

    public override int GetMaxTeamSize(int baseSize) => _teamSize;
}
