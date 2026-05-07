using QuestBoard.Api.Patterns.Chain;

namespace QuestBoard.Tests.Patterns;

// [PATTERN: Chain of Responsibility] — Tests for quest validation chain
public class ChainTests
{
    private static IQuestValidationHandler BuildFullChain()
    {
        var title = new TitleValidationHandler();
        var pricing = new PricingValidationHandler();
        var skills = new SkillsValidationHandler();
        var deadline = new DeadlineValidationHandler();

        title.SetNext(pricing).SetNext(skills).SetNext(deadline);
        return title;
    }

    private static QuestValidationContext ValidContext() => new()
    {
        Title = "Build REST API",
        BaseGold = 500m,
        RequiredSkills = new List<string> { "C#" },
        Deadline = DateTime.UtcNow.AddDays(7),
    };

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidQuest_PassesFullChain()
    {
        var chain = BuildFullChain();
        var result = chain.Validate(ValidContext());

        Assert.Null(result);
    }

    // ── TitleValidationHandler ─────────────────────────────────────────────

    [Fact]
    public void EmptyTitle_FailsAtFirstHandler()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.Title = "";

        var result = chain.Validate(ctx);

        Assert.Equal("Quest title is verplicht.", result);
    }

    [Fact]
    public void TitleTooLong_FailsWithLengthError()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.Title = new string('x', 101);

        var result = chain.Validate(ctx);

        Assert.Equal("Quest title mag maximaal 100 tekens bevatten.", result);
    }

    // ── PricingValidationHandler ───────────────────────────────────────────

    [Fact]
    public void ZeroGold_FailsAtPricingHandler()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.BaseGold = 0m;

        var result = chain.Validate(ctx);

        Assert.Equal("Quest beloning (BaseGold) moet groter zijn dan 0.", result);
    }

    [Fact]
    public void NegativeGold_FailsAtPricingHandler()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.BaseGold = -100m;

        var result = chain.Validate(ctx);

        Assert.Equal("Quest beloning (BaseGold) moet groter zijn dan 0.", result);
    }

    // ── SkillsValidationHandler ────────────────────────────────────────────

    [Fact]
    public void NoSkills_FailsAtSkillsHandler()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.RequiredSkills = new List<string>();

        var result = chain.Validate(ctx);

        Assert.Equal("Een quest vereist minimaal één skill.", result);
    }

    // ── DeadlineValidationHandler ──────────────────────────────────────────

    [Fact]
    public void PastDeadline_FailsAtDeadlineHandler()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.Deadline = DateTime.UtcNow.AddDays(-1);

        var result = chain.Validate(ctx);

        Assert.Equal("Deadline moet in de toekomst liggen.", result);
    }

    [Fact]
    public void UrgentWithoutDeadline_FailsAtDeadlineHandler()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.Deadline = null;
        ctx.IsUrgent = true;

        var result = chain.Validate(ctx);

        Assert.Equal("Een urgente quest vereist een deadline.", result);
    }

    [Fact]
    public void NoDeadlineNotUrgent_PassesChain()
    {
        var chain = BuildFullChain();
        var ctx = ValidContext();
        ctx.Deadline = null;
        ctx.IsUrgent = false;

        var result = chain.Validate(ctx);

        Assert.Null(result);
    }

    // ── Short-circuit behaviour ────────────────────────────────────────────

    [Fact]
    public void MultipleErrors_ReturnsFirstFailureOnly()
    {
        var chain = BuildFullChain();
        var ctx = new QuestValidationContext
        {
            Title = "",           // fails first
            BaseGold = 0m,        // would also fail
            RequiredSkills = new List<string>(),  // would also fail
        };

        var result = chain.Validate(ctx);

        // Chain stops at first failure — only title error returned
        Assert.Equal("Quest title is verplicht.", result);
    }

    // ── Single-handler chain ───────────────────────────────────────────────

    [Fact]
    public void SingleHandler_ReturnsNullWhenValid()
    {
        var handler = new TitleValidationHandler();
        var ctx = ValidContext();

        var result = handler.Validate(ctx);

        Assert.Null(result);
    }
}
