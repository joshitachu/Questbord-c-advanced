using QuestBoard.Api.Patterns.Interpreter;

namespace QuestBoard.Tests.Patterns;

public class InterpreterTests
{
    private readonly AchievementContext _context = new(new Dictionary<string, double>
    {
        ["quests.completed"] = 15,
        ["rating.avg"] = 4.7,
        ["level"] = 8,
        ["gold"] = 5000,
        ["xp"] = 12000,
        ["skills.count"] = 6,
        ["badges.count"] = 3
    });

    [Fact]
    public void SimpleComparison_GreaterThanOrEqual_True()
    {
        // Arrange
        var expression = new ComparisonExpression("quests.completed", ">=", 10);

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "quests.completed (15) should be >= 10.");
    }

    [Fact]
    public void SimpleComparison_GreaterThan_False()
    {
        // Arrange
        var expression = new ComparisonExpression("level", ">", 10);

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.False(result, "level (8) should NOT be > 10.");
    }

    [Fact]
    public void AndExpression_BothTrue_ReturnsTrue()
    {
        // Arrange
        var left = new ComparisonExpression("quests.completed", ">=", 10);
        var right = new ComparisonExpression("rating.avg", ">", 4.5);
        var expression = new AndExpression(left, right);

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "quests.completed >= 10 AND rating.avg > 4.5 should both be true.");
    }

    [Fact]
    public void AndExpression_OneFalse_ReturnsFalse()
    {
        // Arrange
        var left = new ComparisonExpression("quests.completed", ">=", 10);
        var right = new ComparisonExpression("level", ">", 10);
        var expression = new AndExpression(left, right);

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.False(result, "quests.completed >= 10 is true BUT level > 10 is false, so AND should be false.");
    }

    [Fact]
    public void OrExpression_OneTrue_ReturnsTrue()
    {
        // Arrange
        var left = new ComparisonExpression("level", ">", 10);
        var right = new ComparisonExpression("gold", ">=", 5000);
        var expression = new OrExpression(left, right);

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "level > 10 is false BUT gold >= 5000 is true, so OR should be true.");
    }

    [Fact]
    public void ComplexRule_AndWithOr()
    {
        // Arrange — "quests.completed >= 5 and level >= 3 or gold >= 999999"
        // AND part: quests.completed(15) >= 5 AND level(8) >= 3 → true
        // OR right: gold(5000) >= 999999 → false
        // Result: true OR false → true
        var andLeft = new ComparisonExpression("quests.completed", ">=", 5);
        var andRight = new ComparisonExpression("level", ">=", 3);
        var andExpr = new AndExpression(andLeft, andRight);
        var orRight = new ComparisonExpression("gold", ">=", 999999);
        var expression = new OrExpression(andExpr, orRight);

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "AND part is true, so the whole OR expression should be true.");
    }

    [Fact]
    public void Parser_ParsesSimpleRule()
    {
        // Arrange
        var expression = AchievementRuleParser.Parse("quests.completed >= 10");

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "Parser should handle a simple comparison rule.");
    }

    [Fact]
    public void Parser_ParsesAndRule()
    {
        // Arrange
        var expression = AchievementRuleParser.Parse("quests.completed >= 10 and rating.avg > 4.5");

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "Parser should handle an AND rule where both comparisons are true.");
    }

    [Fact]
    public void Parser_ParsesOrRule()
    {
        // Arrange
        var expression = AchievementRuleParser.Parse("level > 10 or gold >= 5000");

        // Act
        var result = expression.Interpret(_context);

        // Assert
        Assert.True(result, "Parser should handle an OR rule where at least one comparison is true.");
    }
}
