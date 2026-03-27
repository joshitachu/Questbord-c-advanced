namespace QuestBoard.Api.Patterns.Interpreter;

// [EIGEN INBRENG: DSL Achievement Engine]
// Zelfontworpen Domain-Specific Language (DSL) waarmee achievement-regels als leesbare strings
// worden gedefinieerd (bv. "quests.completed >= 10 and rating.avg >= 4.5").
// De parser bouwt een expression tree op (Composite pattern) met AND/OR-precedentie,
// die vervolgens wordt geevalueerd tegen freelancer-statistieken via het Interpreter pattern.
// Dit maakt het mogelijk om nieuwe achievements toe te voegen zonder code-wijzigingen.

// [PATTERN: Interpreter] Parser — zet DSL string om naar expression tree
/// <summary>
/// Parses achievement DSL rule strings into an expression tree.
/// Supports comparisons (property operator value), "and" / "or" keywords.
/// OR has lower precedence than AND — evaluated left-to-right without parentheses.
///
/// Examples:
///   "quests.completed >= 10"
///   "quests.completed >= 10 and rating.avg > 4.5"
///   "level >= 5 or quests.completed >= 20"
///   "quests.completed >= 5 and level >= 3 or gold >= 1000"
///     → OrExpression(AndExpression(comp1, comp2), comp3)
/// </summary>
public static class AchievementRuleParser
{
    public static IExpression Parse(string rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
            throw new ArgumentException("Rule cannot be null or empty.", nameof(rule));

        // 1. Split on " or " first — each part is an OR operand
        var orParts = rule.Split(" or ", StringSplitOptions.TrimEntries);

        // 2. Parse each OR operand (which may contain AND expressions)
        var orExpressions = orParts.Select(ParseAndGroup).ToArray();

        // 3. Build OR expression tree left-to-right
        var result = orExpressions[0];
        for (var i = 1; i < orExpressions.Length; i++)
        {
            result = new OrExpression(result, orExpressions[i]);
        }

        return result;
    }

    private static IExpression ParseAndGroup(string andGroup)
    {
        // Split on " and " — each part is an AND operand (a single comparison)
        var andParts = andGroup.Split(" and ", StringSplitOptions.TrimEntries);

        var andExpressions = andParts.Select(ParseComparison).ToArray();

        // Build AND expression tree left-to-right
        IExpression result = andExpressions[0];
        for (var i = 1; i < andExpressions.Length; i++)
        {
            result = new AndExpression(result, andExpressions[i]);
        }

        return result;
    }

    private static ComparisonExpression ParseComparison(string comparison)
    {
        // Expected format: "property operator value" (e.g., "quests.completed >= 10")
        var tokens = comparison.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != 3)
            throw new FormatException(
                $"Invalid comparison expression: '{comparison}'. Expected format: 'property operator value'.");

        var property = tokens[0];
        var op = tokens[1];
        if (!double.TryParse(tokens[2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException(
                $"Invalid numeric value '{tokens[2]}' in comparison: '{comparison}'.");
        }

        return new ComparisonExpression(property, op, value);
    }
}
