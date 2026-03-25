// [PATTERN: Flyweight]
// Immutable shared object representing a skill.
// Instances are cached and reused by SkillFactory to minimize memory allocation.

namespace QuestBoard.Api.Patterns.Flyweight;

public sealed class SkillFlyweight
{
    public string Name { get; }
    public string Category { get; }
    public string IconUrl { get; }

    public SkillFlyweight(string name, string category, string iconUrl)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        IconUrl = iconUrl ?? throw new ArgumentNullException(nameof(iconUrl));
    }

    public override bool Equals(object? obj)
    {
        if (obj is SkillFlyweight other)
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        return false;
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }

    public override string ToString()
    {
        return $"Skill[{Name}, Category={Category}]";
    }
}
