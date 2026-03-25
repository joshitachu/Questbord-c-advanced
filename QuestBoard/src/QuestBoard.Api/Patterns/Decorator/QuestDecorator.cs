namespace QuestBoard.Api.Patterns.Decorator;

// [PATTERN: Decorator] Abstracte decorator — delegeert standaard naar het inner object
public abstract class QuestDecorator : IQuestBehavior
{
    protected readonly IQuestBehavior _inner;

    protected QuestDecorator(IQuestBehavior inner)
    {
        _inner = inner;
    }

    public virtual decimal CalculateGold(decimal baseGold) => _inner.CalculateGold(baseGold);

    public virtual int CalculateXp(int baseXp) => _inner.CalculateXp(baseXp);

    public virtual List<string> GetTags() => _inner.GetTags();

    public virtual string GetDescription(string baseDescription) => _inner.GetDescription(baseDescription);

    public virtual int GetMaxTeamSize(int baseSize) => _inner.GetMaxTeamSize(baseSize);
}
