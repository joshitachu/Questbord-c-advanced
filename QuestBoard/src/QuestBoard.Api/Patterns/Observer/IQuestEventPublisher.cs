namespace QuestBoard.Api.Patterns.Observer;

public interface IQuestEventPublisher
{
    void Subscribe(IQuestEventSubscriber subscriber);
    void Unsubscribe(IQuestEventSubscriber subscriber);
    void Notify(QuestCompletedEvent evt);
    IReadOnlyList<IQuestEventSubscriber> GetSubscribers();
}
