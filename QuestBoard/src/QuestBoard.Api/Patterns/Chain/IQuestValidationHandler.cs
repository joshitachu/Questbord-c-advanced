namespace QuestBoard.Api.Patterns.Chain;

public interface IQuestValidationHandler
{
    IQuestValidationHandler SetNext(IQuestValidationHandler next);
    string? Validate(QuestValidationContext context);
}
