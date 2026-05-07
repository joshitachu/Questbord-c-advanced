using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Api.Patterns.Command;

// [PATTERN: Command] — Marks an in-progress quest complete and awards XP/gold to the freelancer
public class CompleteQuestCommand : IQuestCommand
{
    private readonly Quest _quest;
    private readonly Freelancer _freelancer;
    private QuestStatus _previousStatus;
    private DateTime? _previousCompletedAt;
    private int _previousXp;
    private decimal _previousGold;
    private int _previousQuestsCompleted;

    public string CommandName => "CompleteQuest";

    public CompleteQuestCommand(Quest quest, Freelancer freelancer)
    {
        _quest = quest;
        _freelancer = freelancer;
    }

    public void Execute()
    {
        if (_quest.Status != QuestStatus.InProgress)
            throw new InvalidOperationException($"Quest '{_quest.Title}' is not in progress.");

        if (_quest.AssignedFreelancerId != _freelancer.Id)
            throw new InvalidOperationException("Freelancer is not assigned to this quest.");

        _previousStatus = _quest.Status;
        _previousCompletedAt = _quest.CompletedAt;
        _previousXp = _freelancer.Xp;
        _previousGold = _freelancer.Gold;
        _previousQuestsCompleted = _freelancer.QuestsCompleted;

        _quest.Status = QuestStatus.Completed;
        _quest.CompletedAt = DateTime.UtcNow;

        _freelancer.Xp += _quest.BaseXp;
        _freelancer.Gold += _quest.BaseGold;
        _freelancer.QuestsCompleted++;

        if (!_freelancer.CompletedQuestIds.Contains(_quest.Id))
            _freelancer.CompletedQuestIds.Add(_quest.Id);
    }

    public void Undo()
    {
        _quest.Status = _previousStatus;
        _quest.CompletedAt = _previousCompletedAt;

        _freelancer.Xp = _previousXp;
        _freelancer.Gold = _previousGold;
        _freelancer.QuestsCompleted = _previousQuestsCompleted;
        _freelancer.CompletedQuestIds.Remove(_quest.Id);
    }
}
