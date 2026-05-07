namespace QuestBoard.Api.Patterns.Chain;

public class SkillsValidationHandler : QuestValidationHandler
{
    public override string? Validate(QuestValidationContext context)
    {
        if (context.RequiredSkills.Count == 0)
            return "Een quest vereist minimaal één skill.";

        return base.Validate(context);
    }
}
