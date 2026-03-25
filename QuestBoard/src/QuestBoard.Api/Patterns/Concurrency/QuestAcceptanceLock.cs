using System.Collections.Concurrent;

namespace QuestBoard.Api.Patterns.Concurrency;

// [PATTERN: Monitor] — Concurrency pattern
// Lost race condition op in AcceptQuest() waar twee threads tegelijk
// dezelfde quest kunnen accepteren (check-then-act op quest.Status).
// Gebruikt per-quest SemaphoreSlim(1,1) als mutex.
public class QuestAcceptanceLock
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(Guid questId)
    {
        return _locks.GetOrAdd(questId, _ => new SemaphoreSlim(1, 1));
    }

    public async Task<T> ExecuteWithLockAsync<T>(Guid questId, Func<Task<T>> action)
    {
        var semaphore = GetLock(questId);
        await semaphore.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public T ExecuteWithLock<T>(Guid questId, Func<T> action)
    {
        var semaphore = GetLock(questId);
        semaphore.Wait();
        try
        {
            return action();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public bool TryRemoveLock(Guid questId)
    {
        return _locks.TryRemove(questId, out _);
    }
}
