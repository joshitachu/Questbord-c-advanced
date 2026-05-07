namespace QuestBoard.Api.Patterns.Command;

// [PATTERN: Command] — Executes commands and maintains an undo history stack
public class QuestCommandInvoker
{
    private readonly Stack<IQuestCommand> _history = new();

    public void Execute(IQuestCommand command)
    {
        command.Execute();
        _history.Push(command);
    }

    public void Undo()
    {
        if (_history.Count == 0)
            throw new InvalidOperationException("No commands to undo.");

        _history.Pop().Undo();
    }

    public int HistoryCount => _history.Count;

    public IReadOnlyCollection<string> GetHistory() =>
        _history.Select(c => c.CommandName).ToList().AsReadOnly();
}
