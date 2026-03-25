namespace QuestBoard.Api.Patterns.Interpreter;

/// <summary>
/// Context containing evaluatable freelancer stats for achievement rule interpretation.
/// Typical keys: "quests.completed", "rating.avg", "level", "gold", "xp", "skills.count", "badges.count"
/// </summary>
public class AchievementContext
{
    private readonly Dictionary<string, double> _properties;

    public AchievementContext(Dictionary<string, double> properties)
    {
        _properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    /// <summary>
    /// Returns the value for the given key, or 0 if not found.
    /// </summary>
    public double GetValue(string key)
    {
        return _properties.TryGetValue(key, out var value) ? value : 0;
    }
}
