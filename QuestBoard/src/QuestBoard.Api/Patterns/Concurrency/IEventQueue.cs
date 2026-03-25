namespace QuestBoard.Api.Patterns.Concurrency;

// [PATTERN: Producer-Consumer] — Concurrency pattern
// Interface voor een thread-safe event queue.
// Ontkoppelt de HTTP request thread (producer) van de event verwerking (consumer).
public interface IEventQueue<T>
{
    ValueTask EnqueueAsync(T item, CancellationToken ct = default);
    IAsyncEnumerable<T> DequeueAllAsync(CancellationToken ct);
    int Count { get; }
}
