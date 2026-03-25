// [PATTERN: Flyweight]
// Singleton pool that caches BadgeFlyweight instances by name.
// Uses ConcurrentDictionary for thread-safe access in concurrent web request scenarios.

using System.Collections.Concurrent;

namespace QuestBoard.Api.Patterns.Flyweight;

public sealed class BadgeFactory
{
    private static readonly Lazy<BadgeFactory> _instance = new(() => new BadgeFactory());
    private readonly ConcurrentDictionary<string, BadgeFlyweight> _pool = new(StringComparer.OrdinalIgnoreCase);

    private BadgeFactory() { }

    public static BadgeFactory Instance => _instance.Value;

    /// <summary>
    /// Returns a cached BadgeFlyweight for the given name, or creates and caches a new one.
    /// </summary>
    public BadgeFlyweight GetBadge(string name, string description = "", string tier = "Bronze", string iconUrl = "")
    {
        return _pool.GetOrAdd(name, key => new BadgeFlyweight(key, description, tier, iconUrl));
    }

    /// <summary>
    /// Returns the number of unique badges currently in the pool.
    /// </summary>
    public int GetBadgeCount()
    {
        return _pool.Count;
    }

    /// <summary>
    /// Returns all cached badge flyweights.
    /// </summary>
    public IReadOnlyCollection<BadgeFlyweight> GetAllBadges()
    {
        return _pool.Values.ToList().AsReadOnly();
    }
}
