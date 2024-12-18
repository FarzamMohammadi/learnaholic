namespace EnhancedLRUCache.Caching.Payload;

public class CacheItemEventArgs<TKey, TValue>
(
    TKey key,
    TValue? value,
    DateTime timestamp
) : EventArgs
{
    public TKey Key { get; } = key;
    public TValue? Value { get; } = value;
    public DateTime Timestamp { get; } = timestamp;
}