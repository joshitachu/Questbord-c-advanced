using QuestBoard.Api.Patterns.Flyweight;

namespace QuestBoard.Tests.Patterns;

public class FlyweightTests
{
    [Fact]
    public void SkillFactory_ReturnsSameInstance()
    {
        // Arrange
        var factory = SkillFactory.Instance;

        // Act
        var skill1 = factory.GetSkill("C#", "Programming", "/icons/csharp.png");
        var skill2 = factory.GetSkill("C#", "Programming", "/icons/csharp.png");

        // Assert
        Assert.True(ReferenceEquals(skill1, skill2),
            "Factory should return the exact same object instance for the same skill name.");
    }

    [Fact]
    public void SkillFactory_DifferentSkills_DifferentInstances()
    {
        // Arrange
        var factory = SkillFactory.Instance;

        // Act
        var csharp = factory.GetSkill("C#", "Programming", "/icons/csharp.png");
        var python = factory.GetSkill("Python", "Programming", "/icons/python.png");

        // Assert
        Assert.False(ReferenceEquals(csharp, python),
            "Factory should return different instances for different skill names.");
        Assert.NotEqual(csharp.Name, python.Name);
    }

    [Fact]
    public void BadgeFactory_ReturnsSameInstance()
    {
        // Arrange
        var factory = BadgeFactory.Instance;

        // Act
        var badge1 = factory.GetBadge("First Quest", "Completed your first quest", "Bronze", "/icons/first-quest.png");
        var badge2 = factory.GetBadge("First Quest", "Completed your first quest", "Bronze", "/icons/first-quest.png");

        // Assert
        Assert.True(ReferenceEquals(badge1, badge2),
            "Factory should return the exact same object instance for the same badge name.");
    }

    [Fact]
    public void SkillFactory_GetSkillCount_ReturnsCorrectCount()
    {
        // Arrange
        var factory = SkillFactory.Instance;

        // Act — add skills that are guaranteed unique to this test
        factory.GetSkill("CountTest_JavaScript", "Programming", "/icons/js.png");
        factory.GetSkill("CountTest_TypeScript", "Programming", "/icons/ts.png");
        factory.GetSkill("CountTest_Rust", "Systems", "/icons/rust.png");

        // Assert
        var count = factory.GetSkillCount();
        Assert.True(count >= 3,
            $"Pool should contain at least 3 skills, but found {count}.");
    }

    [Fact]
    public void BadgeFactory_PoolSharing_MultipleRequests()
    {
        // Arrange
        var factory = BadgeFactory.Instance;
        var initialCount = factory.GetBadgeCount();

        // Act — request the same badge 100 times
        for (var i = 0; i < 100; i++)
        {
            factory.GetBadge("Pool Sharing Test Badge", "Awarded for pool sharing", "Gold", "/icons/pool.png");
        }

        // Assert — only one new badge should have been created
        var newBadges = factory.GetBadgeCount() - initialCount;
        Assert.Equal(1, newBadges);
    }
}
