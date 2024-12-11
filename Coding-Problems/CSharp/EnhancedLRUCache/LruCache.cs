namespace EnhancedLRUCache;

public interface ILruCache<TKey, TValue>
{
    public void Put(TKey key, TValue value, TimeSpan? ttl);
    public bool TryGet(TKey key, out TValue value);
    public void Remove(TKey key);
    public void Clear();
}

public class LruCache<TKey, TValue>
(
    ILruStorage<TKey, TValue> storage
) : ILruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ILruStorage<TKey, TValue> _storage = storage ?? throw new ArgumentNullException(nameof(storage));

    public void Put(TKey key, TValue value, TimeSpan? ttl)
    {
        ThrowIfKeyIsNull(key);
    }


    public bool TryGet(TKey key, out TValue value)
    {
        ThrowIfKeyIsNull(key);

        return _storage.TryGet(key, out value);
    }

    public void Remove(TKey key)
    {
        ThrowIfKeyIsNull(key);

        _storage.Remove(key);
    }

    public void Clear()
    {
        _storage.Clear();
    }

    private static void ThrowIfKeyIsNull(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
    }
}