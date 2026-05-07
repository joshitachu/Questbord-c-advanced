namespace QuestBoard.Api.Patterns.Command;

// [PATTERN: Command] — Encapsulates a quest lifecycle action as an object
public interface IQuestCommand
{
    string CommandName { get; }
    void Execute();
    void Undo();
}
