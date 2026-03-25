using QuestBoard.Api.Patterns.Observer;

namespace QuestBoard.Api.Patterns.Concurrency;

// [PATTERN: Producer-Consumer] — Concurrency pattern (Consumer)
// BackgroundService die continu events van de queue leest en doorgeeft
// aan de Observer publisher. Events worden nu verwerkt op een background thread
// in plaats van synchroon op de HTTP request thread.
public class EventProcessorService : BackgroundService
{
    private readonly IEventQueue<QuestCompletedEvent> _eventQueue;
    private readonly IQuestEventPublisher _publisher;
    private readonly ILogger<EventProcessorService> _logger;

    public EventProcessorService(
        IEventQueue<QuestCompletedEvent> eventQueue,
        IQuestEventPublisher publisher,
        ILogger<EventProcessorService> logger)
    {
        _eventQueue = eventQueue;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventProcessorService gestart — wacht op events in de queue.");

        await foreach (var evt in _eventQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation(
                    "Event verwerken: Quest '{Title}' voltooid door {Freelancer}",
                    evt.Quest.Title, evt.Freelancer.Name);

                _publisher.Notify(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwerken van event voor quest '{Title}'", evt.Quest.Title);
            }
        }

        _logger.LogInformation("EventProcessorService gestopt.");
    }
}
