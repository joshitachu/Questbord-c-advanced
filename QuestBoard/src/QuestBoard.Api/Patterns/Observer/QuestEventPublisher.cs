namespace QuestBoard.Api.Patterns.Observer;

// [PATTERN: Observer] — Behavioral pattern
// Beheert subscriber registraties en stuurt events door naar alle geabonneerde observers
public class QuestEventPublisher : IQuestEventPublisher
{
    private readonly List<IQuestEventSubscriber> _subscribers = new();

    public void Subscribe(IQuestEventSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
            _subscribers.Add(subscriber);
    }

    public void Unsubscribe(IQuestEventSubscriber subscriber)
    {
        _subscribers.Remove(subscriber);
    }

    public void Notify(QuestCompletedEvent evt)
    {
        foreach (var subscriber in _subscribers)
        {
            subscriber.OnQuestCompleted(evt);
        }
    }

    public IReadOnlyList<IQuestEventSubscriber> GetSubscribers()
    {
        return _subscribers.AsReadOnly();
    }
}
