namespace QuestBoard.Api.Patterns.Decorator;

// [PATTERN: Decorator] Featured bonus: 1.2x gold, +50 XP
public class FeaturedQuestDecorator : QuestDecorator
{
    public FeaturedQuestDecorator(IQuestBehavior inner) : base(inner) { }

    public override decimal CalculateGold(decimal baseGold) =>
        _inner.CalculateGold(baseGold) * 1.2m;

    public override int CalculateXp(int baseXp) =>
        _inner.CalculateXp(baseXp) + 50;

    public override List<string> GetTags()
    {
        var tags = _inner.GetTags();
        tags.Add("FEATURED");
        return tags;
    }

    public override string GetDescription(string baseDescription) =>
        "[FEATURED] " + _inner.GetDescription(baseDescription);
}
