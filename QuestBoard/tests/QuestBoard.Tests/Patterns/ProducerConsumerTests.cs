using QuestBoard.Api.Patterns.Concurrency;

namespace QuestBoard.Tests.Patterns;

public class ProducerConsumerTests
{
    [Fact]
    public async Task EnqueueDequeue_Roundtrip_ReturnsItem()
    {
        // Arrange
        var queue = new EventQueue<string>(10);

        // Act
        await queue.EnqueueAsync("event1");

        // Assert
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await foreach (var item in queue.DequeueAllAsync(cts.Token))
        {
            Assert.Equal("event1", item);
            break; // alleen eerste item
        }
    }

    [Fact]
    public async Task DequeueAll_MaintainsFIFOOrder()
    {
        // Arrange
        var queue = new EventQueue<int>(10);
        await queue.EnqueueAsync(1);
        await queue.EnqueueAsync(2);
        await queue.EnqueueAsync(3);

        // Act
        var results = new List<int>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await foreach (var item in queue.DequeueAllAsync(cts.Token))
        {
            results.Add(item);
            if (results.Count == 3) break;
        }

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, results);
    }

    [Fact]
    public async Task Count_ReflectsQueueState()
    {
        // Arrange
        var queue = new EventQueue<string>(10);

        // Act & Assert
        Assert.Equal(0, queue.Count);

        await queue.EnqueueAsync("a");
        Assert.Equal(1, queue.Count);

        await queue.EnqueueAsync("b");
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public async Task MultipleProducers_AllItemsDequeued()
    {
        // Arrange
        var queue = new EventQueue<int>(100);
        var producerCount = 5;
        var itemsPerProducer = 10;

        // Act — 5 producers schrijven elk 10 items
        var tasks = Enumerable.Range(0, producerCount)
            .Select(p => Task.Run(async () =>
            {
                for (int i = 0; i < itemsPerProducer; i++)
                    await queue.EnqueueAsync(p * 100 + i);
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert — alle 50 items moeten in de queue zitten
        Assert.Equal(50, queue.Count);

        // Dequeue alle items
        var results = new List<int>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await foreach (var item in queue.DequeueAllAsync(cts.Token))
        {
            results.Add(item);
            if (results.Count == 50) break;
        }

        Assert.Equal(50, results.Count);
        Assert.Equal(50, results.Distinct().Count()); // alle uniek
    }
}
