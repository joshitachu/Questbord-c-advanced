using System.Threading.Channels;

namespace QuestBoard.Api.Patterns.Concurrency;

// [PATTERN: Producer-Consumer] — Concurrency pattern
// Thread-safe bounded queue op basis van System.Threading.Channels.
// Producer (HTTP thread): EnqueueAsync() → voegt events toe en returnt direct.
// Consumer (BackgroundService): DequeueAllAsync() → leest continu events voor verwerking.
public class EventQueue<T> : IEventQueue<T>
{
    private readonly Channel<T> _channel;

    public EventQueue(int capacity = 100)
    {
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(T item, CancellationToken ct = default)
    {
        return _channel.Writer.WriteAsync(item, ct);
    }

    public IAsyncEnumerable<T> DequeueAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public int Count => _channel.Reader.Count;
}
