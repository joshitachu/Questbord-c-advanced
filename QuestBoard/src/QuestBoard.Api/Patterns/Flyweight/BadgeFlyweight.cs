// [PATTERN: Flyweight]
// Immutable shared object representing a badge/achievement icon.
// Instances are cached and reused by BadgeFactory to minimize memory allocation.

namespace QuestBoard.Api.Patterns.Flyweight;

public sealed class BadgeFlyweight
{
    public string Name { get; }
    public string Description { get; }
    public string Tier { get; }
    public string IconUrl { get; }

    public BadgeFlyweight(string name, string description, string tier, string iconUrl)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Tier = tier ?? throw new ArgumentNullException(nameof(tier));
        IconUrl = iconUrl ?? throw new ArgumentNullException(nameof(iconUrl));
    }

    public override bool Equals(object? obj)
    {
        if (obj is BadgeFlyweight other)
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        return false;
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }

    public override string ToString()
    {
        return $"Badge[{Name}, Tier={Tier}]";
    }
}
