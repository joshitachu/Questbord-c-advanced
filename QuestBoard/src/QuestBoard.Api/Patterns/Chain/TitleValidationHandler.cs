namespace QuestBoard.Api.Patterns.Chain;

public class TitleValidationHandler : QuestValidationHandler
{
    public override string? Validate(QuestValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Title))
            return "Quest title is verplicht.";

        if (context.Title.Length > 100)
            return "Quest title mag maximaal 100 tekens bevatten.";

        return base.Validate(context);
    }
}
