namespace QuestBoard.Api.Patterns.Chain;

public class DeadlineValidationHandler : QuestValidationHandler
{
    public override string? Validate(QuestValidationContext context)
    {
        if (context.Deadline.HasValue && context.Deadline.Value <= DateTime.UtcNow)
            return "Deadline moet in de toekomst liggen.";

        if (context.IsUrgent && !context.Deadline.HasValue)
            return "Een urgente quest vereist een deadline.";

        return base.Validate(context);
    }
}
