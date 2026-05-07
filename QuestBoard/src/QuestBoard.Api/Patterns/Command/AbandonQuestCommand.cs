using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Api.Patterns.Command;

// [PATTERN: Command] — Returns an in-progress quest to Open status, unassigning the freelancer
public class AbandonQuestCommand : IQuestCommand
{
    private readonly Quest _quest;
    private QuestStatus _previousStatus;
    private Guid? _previousFreelancerId;

    public string CommandName => "AbandonQuest";

    public AbandonQuestCommand(Quest quest)
    {
        _quest = quest;
    }

    public void Execute()
    {
        if (_quest.Status != QuestStatus.InProgress)
            throw new InvalidOperationException($"Quest '{_quest.Title}' cannot be abandoned from status '{_quest.Status}'.");

        _previousStatus = _quest.Status;
        _previousFreelancerId = _quest.AssignedFreelancerId;

        _quest.Status = QuestStatus.Open;
        _quest.AssignedFreelancerId = null;
    }

    public void Undo()
    {
        _quest.Status = _previousStatus;
        _quest.AssignedFreelancerId = _previousFreelancerId;
    }
}
