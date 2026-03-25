namespace QuestBoard.Api.Patterns.Interpreter;

// [PATTERN: Interpreter] Non-terminal expression — logische AND combinatie
/// <summary>
/// Non-terminal expression that combines two expressions with logical AND.
/// </summary>
public class AndExpression : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    public AndExpression(IExpression left, IExpression right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public bool Interpret(AchievementContext context)
    {
        return _left.Interpret(context) && _right.Interpret(context);
    }
}
