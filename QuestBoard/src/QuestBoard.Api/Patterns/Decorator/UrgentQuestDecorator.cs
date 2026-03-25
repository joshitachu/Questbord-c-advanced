namespace QuestBoard.Api.Patterns.Decorator;

public class UrgentQuestDecorator : QuestDecorator
{
    public UrgentQuestDecorator(IQuestBehavior inner) : base(inner) { }

    public override decimal CalculateGold(decimal baseGold) =>
        _inner.CalculateGold(baseGold) * 1.5m;

    public override int CalculateXp(int baseXp) =>
        _inner.CalculateXp(baseXp) * 2;

    public override List<string> GetTags()
    {
        var tags = _inner.GetTags();
        tags.Add("URGENT");
        return tags;
    }

    public override string GetDescription(string baseDescription) =>
        "[URGENT] " + _inner.GetDescription(baseDescription);
}
