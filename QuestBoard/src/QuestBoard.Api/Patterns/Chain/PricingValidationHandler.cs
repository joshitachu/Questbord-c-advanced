namespace QuestBoard.Api.Patterns.Chain;

public class PricingValidationHandler : QuestValidationHandler
{
    public override string? Validate(QuestValidationContext context)
    {
        if (context.BaseGold <= 0)
            return "Quest beloning (BaseGold) moet groter zijn dan 0.";

        return base.Validate(context);
    }
}
