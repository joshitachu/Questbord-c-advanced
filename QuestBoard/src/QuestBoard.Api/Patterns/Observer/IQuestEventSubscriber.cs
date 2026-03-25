namespace QuestBoard.Api.Patterns.Observer;

public interface IQuestEventSubscriber
{
    string Name { get; }
    void OnQuestCompleted(QuestCompletedEvent evt);
}
