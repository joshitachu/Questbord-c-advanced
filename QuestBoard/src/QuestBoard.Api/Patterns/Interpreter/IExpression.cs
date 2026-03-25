namespace QuestBoard.Api.Patterns.Interpreter;

// [PATTERN: Interpreter] — Behavioral pattern
// Evalueert achievement DSL regels zoals: "quests.completed >= 10 and rating.avg > 4.5"
public interface IExpression
{
    bool Interpret(AchievementContext context);
}
