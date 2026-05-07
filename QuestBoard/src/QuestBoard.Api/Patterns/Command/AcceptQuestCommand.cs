using QuestBoard.Api.Models.Domain;

namespace QuestBoard.Api.Patterns.Command;

// [PATTERN: Command] — Assigns a freelancer to an open quest, reversible via Undo()
public class AcceptQuestCommand : IQuestCommand
{
    private readonly Quest _quest;
    private readonly Freelancer _freelancer;
    private Guid? _previousFreelancerId;
    private QuestStatus _previousStatus;

    public string CommandName => "AcceptQuest";

    public AcceptQuestCommand(Quest quest, Freelancer freelancer)
    {
        _quest = quest;
        _freelancer = freelancer;
    }

    public void Execute()
    {
        if (_quest.Status != QuestStatus.Open)
            throw new InvalidOperationException($"Quest '{_quest.Title}' is not open for acceptance.");

        _previousFreelancerId = _quest.AssignedFreelancerId;
        _previousStatus = _quest.Status;

        _quest.AssignedFreelancerId = _freelancer.Id;
        _quest.Status = QuestStatus.InProgress;
    }

    public void Undo()
    {
        _quest.AssignedFreelancerId = _previousFreelancerId;
        _quest.Status = _previousStatus;
    }
}
