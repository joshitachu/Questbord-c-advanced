using QuestBoard.Api.Patterns.Command;
using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Tests.Patterns;

// [PATTERN: Command] — Tests for quest lifecycle commands and undo/redo history
public class CommandTests
{
    private static Quest BuildOpenQuest() => new()
    {
        Title = "Slay the Dragon",
        Status = QuestStatus.Open,
        BaseXp = 500,
        BaseGold = 250m,
    };

    private static Freelancer BuildFreelancer() => new()
    {
        Name = "Arthur",
        Xp = 100,
        Gold = 50m,
        QuestsCompleted = 0,
    };

    // ── AcceptQuestCommand ─────────────────────────────────────────────────

    [Fact]
    public void AcceptQuest_SetsStatusInProgressAndAssignsFreelancer()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));

        Assert.Equal(QuestStatus.InProgress, quest.Status);
        Assert.Equal(freelancer.Id, quest.AssignedFreelancerId);
    }

    [Fact]
    public void AcceptQuest_ThrowsWhenQuestNotOpen()
    {
        var quest = BuildOpenQuest();
        quest.Status = QuestStatus.InProgress;
        var freelancer = BuildFreelancer();

        var cmd = new AcceptQuestCommand(quest, freelancer);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    [Fact]
    public void AcceptQuest_UndoRestoresOpenStatus()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));
        invoker.Undo();

        Assert.Equal(QuestStatus.Open, quest.Status);
        Assert.Null(quest.AssignedFreelancerId);
    }

    // ── CompleteQuestCommand ───────────────────────────────────────────────

    [Fact]
    public void CompleteQuest_AwardsXpAndGoldToFreelancer()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));
        invoker.Execute(new CompleteQuestCommand(quest, freelancer));

        Assert.Equal(QuestStatus.Completed, quest.Status);
        Assert.NotNull(quest.CompletedAt);
        Assert.Equal(600, freelancer.Xp);  // 100 + 500
        Assert.Equal(300m, freelancer.Gold); // 50 + 250
        Assert.Equal(1, freelancer.QuestsCompleted);
        Assert.Contains(quest.Id, freelancer.CompletedQuestIds);
    }

    [Fact]
    public void CompleteQuest_ThrowsWhenNotInProgress()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        quest.AssignedFreelancerId = freelancer.Id;

        var cmd = new CompleteQuestCommand(quest, freelancer);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    [Fact]
    public void CompleteQuest_UndoRestoresFreelancerStats()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));
        invoker.Execute(new CompleteQuestCommand(quest, freelancer));
        invoker.Undo(); // undo complete

        Assert.Equal(QuestStatus.InProgress, quest.Status);
        Assert.Equal(100, freelancer.Xp);
        Assert.Equal(50m, freelancer.Gold);
        Assert.Equal(0, freelancer.QuestsCompleted);
        Assert.DoesNotContain(quest.Id, freelancer.CompletedQuestIds);
    }

    // ── AbandonQuestCommand ────────────────────────────────────────────────

    [Fact]
    public void AbandonQuest_ResetsQuestToOpen()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));
        invoker.Execute(new AbandonQuestCommand(quest));

        Assert.Equal(QuestStatus.Open, quest.Status);
        Assert.Null(quest.AssignedFreelancerId);
    }

    [Fact]
    public void AbandonQuest_ThrowsWhenQuestNotInProgress()
    {
        var quest = BuildOpenQuest();

        var cmd = new AbandonQuestCommand(quest);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    [Fact]
    public void AbandonQuest_UndoRestoresInProgressState()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));
        invoker.Execute(new AbandonQuestCommand(quest));
        invoker.Undo();

        Assert.Equal(QuestStatus.InProgress, quest.Status);
        Assert.Equal(freelancer.Id, quest.AssignedFreelancerId);
    }

    // ── QuestCommandInvoker ────────────────────────────────────────────────

    [Fact]
    public void Invoker_TracksCommandHistory()
    {
        var quest = BuildOpenQuest();
        var freelancer = BuildFreelancer();
        var invoker = new QuestCommandInvoker();

        invoker.Execute(new AcceptQuestCommand(quest, freelancer));
        invoker.Execute(new AbandonQuestCommand(quest));

        Assert.Equal(2, invoker.HistoryCount);
        var history = invoker.GetHistory().ToList();
        // Stack iterates LIFO: last executed command appears first
        Assert.Equal("AbandonQuest", history[0]);
        Assert.Equal("AcceptQuest", history[1]);
    }

    [Fact]
    public void Invoker_UndoThrowsWhenHistoryEmpty()
    {
        var invoker = new QuestCommandInvoker();

        Assert.Throws<InvalidOperationException>(() => invoker.Undo());
    }
}
