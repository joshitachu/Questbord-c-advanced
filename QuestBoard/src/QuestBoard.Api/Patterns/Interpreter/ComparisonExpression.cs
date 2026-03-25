namespace QuestBoard.Api.Patterns.Interpreter;

/// <summary>
/// Terminal expression that compares a context property against a value using a comparison operator.
/// Supported operators: ">", ">=", "&lt;", "&lt;=", "=="
/// </summary>
public class ComparisonExpression : IExpression
{
    private readonly string _propertyName;
    private readonly string _op;
    private readonly double _value;

    public ComparisonExpression(string propertyName, string op, double value)
    {
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _op = op ?? throw new ArgumentNullException(nameof(op));
        _value = value;
    }

    public bool Interpret(AchievementContext context)
    {
        var actual = context.GetValue(_propertyName);

        return _op switch
        {
            ">"  => actual > _value,
            ">=" => actual >= _value,
            "<"  => actual < _value,
            "<=" => actual <= _value,
            "==" => Math.Abs(actual - _value) < 0.0001,
            _    => throw new InvalidOperationException($"Unknown operator: {_op}")
        };
    }
}
