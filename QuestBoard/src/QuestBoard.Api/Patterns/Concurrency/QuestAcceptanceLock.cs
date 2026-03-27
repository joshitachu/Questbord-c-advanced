using System.Collections.Concurrent;

namespace QuestBoard.Api.Patterns.Concurrency;

// [EIGEN INBRENG: Fine-Grained Per-Quest Locking]
// In plaats van een enkele globale lock gebruikt deze klasse een ConcurrentDictionary
// van SemaphoreSlim(1,1) per quest-ID. Hierdoor kunnen meerdere quests tegelijkertijd
// worden geaccepteerd, terwijl race conditions op dezelfde quest worden voorkomen.
// Dit is efficienter dan een globale lock bij hoge concurrency.

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
