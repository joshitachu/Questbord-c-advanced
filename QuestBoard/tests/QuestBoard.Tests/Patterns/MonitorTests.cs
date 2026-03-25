using QuestBoard.Api.Patterns.Concurrency;

namespace QuestBoard.Tests.Patterns;

public class MonitorTests
{
    [Fact]
    public void GetLock_SameQuestId_ReturnsSameSemaphore()
    {
        // Arrange
        var lockManager = new QuestAcceptanceLock();
        var questId = Guid.NewGuid();

        // Act
        var lock1 = lockManager.GetLock(questId);
        var lock2 = lockManager.GetLock(questId);

        // Assert
        Assert.Same(lock1, lock2);
    }

    [Fact]
    public void GetLock_DifferentQuestIds_ReturnsDifferentSemaphores()
    {
        // Arrange
        var lockManager = new QuestAcceptanceLock();
        var quest1 = Guid.NewGuid();
        var quest2 = Guid.NewGuid();

        // Act
        var lock1 = lockManager.GetLock(quest1);
        var lock2 = lockManager.GetLock(quest2);

        // Assert
        Assert.NotSame(lock1, lock2);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_SerializesConcurrentAccess()
    {
        // Arrange
        var lockManager = new QuestAcceptanceLock();
        var questId = Guid.NewGuid();
        var counter = 0;
        var maxConcurrent = 0;
        var currentConcurrent = 0;

        // Act — 20 concurrent tasks die via lock geserialiseerd worden
        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            await lockManager.ExecuteWithLockAsync(questId, async () =>
            {
                var concurrent = Interlocked.Increment(ref currentConcurrent);
                // Bijhouden max gelijktijdigheid
                int oldMax;
                do { oldMax = maxConcurrent; }
                while (concurrent > oldMax && Interlocked.CompareExchange(ref maxConcurrent, concurrent, oldMax) != oldMax);

                await Task.Delay(10); // simuleer werk
                Interlocked.Increment(ref counter);
                Interlocked.Decrement(ref currentConcurrent);
                return true;
            });
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert — alle 20 taken uitgevoerd, max 1 tegelijk
        Assert.Equal(20, counter);
        Assert.Equal(1, maxConcurrent);
    }

    [Fact]
    public void ExecuteWithLock_Synchronous_SerializesAccess()
    {
        // Arrange
        var lockManager = new QuestAcceptanceLock();
        var questId = Guid.NewGuid();
        var results = new List<int>();

        // Act — sequential lock usage
        for (int i = 0; i < 5; i++)
        {
            var val = i;
            lockManager.ExecuteWithLock(questId, () =>
            {
                results.Add(val);
                return val;
            });
        }

        // Assert — FIFO order behouden
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);
    }

    [Fact]
    public void TryRemoveLock_ExistingQuest_ReturnsTrue()
    {
        // Arrange
        var lockManager = new QuestAcceptanceLock();
        var questId = Guid.NewGuid();
        lockManager.GetLock(questId); // maak lock aan

        // Act
        var removed = lockManager.TryRemoveLock(questId);

        // Assert
        Assert.True(removed);
    }
}
