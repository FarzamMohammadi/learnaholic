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
    ILruPolicy policy
) : ILruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ILruStorage<TKey, TValue> _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    private readonly ILruPolicy _policy = policy ?? throw new ArgumentNullException(nameof(policy));

    public void Put(TKey key, TValue? value, TimeSpan? ttl)
    {
        ArgumentNullException.ThrowIfNull(key);

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

        var (itemAdded, error) = _storage.TryPut(key, newCacheEntry);

        if (!itemAdded) throw new InvalidOperationException(error.ToString());
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

        _storage.Remove(key);
    }

    public void Clear()
    {
        _storage.Clear();
    }
}