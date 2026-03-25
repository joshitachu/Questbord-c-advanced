// [PATTERN: Flyweight]
// Singleton pool that caches SkillFlyweight instances by name.
// Uses ConcurrentDictionary for thread-safe access in concurrent web request scenarios.

using System.Collections.Concurrent;

namespace QuestBoard.Api.Patterns.Flyweight;

public sealed class SkillFactory
{
    private static readonly Lazy<SkillFactory> _instance = new(() => new SkillFactory());
    private readonly ConcurrentDictionary<string, SkillFlyweight> _pool = new(StringComparer.OrdinalIgnoreCase);

    private SkillFactory() { }

    public static SkillFactory Instance => _instance.Value;

    /// <summary>
    /// Returns a cached SkillFlyweight for the given name, or creates and caches a new one.
    /// </summary>
    public SkillFlyweight GetSkill(string name, string category = "General", string iconUrl = "")
    {
        return _pool.GetOrAdd(name, key => new SkillFlyweight(key, category, iconUrl));
    }

    /// <summary>
    /// Returns the number of unique skills currently in the pool.
    /// </summary>
    public int GetSkillCount()
    {
        return _pool.Count;
    }

    /// <summary>
    /// Returns all cached skill flyweights.
    /// </summary>
    public IReadOnlyCollection<SkillFlyweight> GetAllSkills()
    {
        return _pool.Values.ToList().AsReadOnly();
    }
}
