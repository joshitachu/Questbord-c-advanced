namespace QuestBoard.Api.Patterns.Chain;

// [PATTERN: Chain of Responsibility] — Abstract base that wires the next handler
public abstract class QuestValidationHandler : IQuestValidationHandler
{
    private IQuestValidationHandler? _next;

    public IQuestValidationHandler SetNext(IQuestValidationHandler next)
    {
        _next = next;
        return next;
    }

    public virtual string? Validate(QuestValidationContext context)
    {
        return _next?.Validate(context);
    }
}
