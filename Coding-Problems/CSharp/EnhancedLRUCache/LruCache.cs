namespace EnhancedLRUCache;

public interface ILruCache<TKey, TValue>
{
    public void Put(TKey key, TValue? value, TimeSpan? ttl);
    public bool TryGet(TKey key, out TValue? value);
    public void Remove(TKey key);
    public void Clear();
}

public class LruCache<TKey, TValue>
(
    ILruStorage<TKey, TValue> storage,
    ILruPolicy policy,
    ICacheStats stats
) : ILruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ILruStorage<TKey, TValue> _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    private readonly ILruPolicy _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    private readonly ICacheStats _stats = stats ?? throw new ArgumentNullException(nameof(stats));

    public void Put(TKey key, TValue? value, TimeSpan? ttl)
    {
        ArgumentNullException.ThrowIfNull(key);

        _stats.IncrementRequestCount();

        DateTime? absoluteExpiration = null;
        TimeSpan? slidingExpiration = null;

        if (_policy.UsesAbsoluteExpiration)
        {
            absoluteExpiration = DateTime.UtcNow + (ttl ?? _policy.DefaultTtl);
        }

        if (_policy.UsesSlidingExpiration)
        {
            slidingExpiration = ttl ?? _policy.DefaultTtl;
        }

        var newCacheEntry = new CacheItem<TValue>(value, absoluteExpiration, slidingExpiration);

        var (itemSuccessfullyAdded, error) = _storage.TryPut(key, newCacheEntry);

        if (!itemSuccessfullyAdded)
        {
            _stats.IncrementMissedRequestCount();

            throw new InvalidOperationException(error.ToString());
        }

        _stats.UpdateItemCount(1);
        _stats.UpdateMemory(newCacheEntry.Size);
    }


    public bool TryGet(TKey key, out TValue? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_storage.TryGet(key, out var cacheItem))
        {
            value = default!;
            return false;
        }

        value = cacheItem.Value;
        return true;
    }

    public void Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        _storage.TryGet(key, out var entry);

        _stats.UpdateItemCount(-1);
        _stats.UpdateMemory(-entry.Size);

        _storage.Remove(key);
    }

    public void Clear()
    {
        _storage.Clear();

        _stats.ClearMetrics();
    }
}