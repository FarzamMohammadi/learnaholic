namespace EnhancedLRUCache;

public interface ICacheItem
{
    public bool IsExpired();
    public void RefreshLastAccessed();
}

public class CacheItem<TValue>
(
    TValue value,
    DateTime? absoluteExpiration = null,
    TimeSpan? slidingExpiration = null
) : ICacheItem
    where TValue : notnull
{
    public TValue Value { get; } = value;
    private DateTime? AbsoluteExpiration { get; } = absoluteExpiration;
    private TimeSpan? SlidingExpiration { get; } = slidingExpiration;
    private DateTime LastAccessed { get; set; } = DateTime.UtcNow;

    public bool IsExpired()
    {
        var now = DateTime.UtcNow;

        if (AbsoluteExpiration.HasValue
            && now > AbsoluteExpiration) return true;

        if (SlidingExpiration.HasValue
            && now > LastAccessed + SlidingExpiration) return true;

        return false;
    }

    public void RefreshLastAccessed()
    {
        LastAccessed = DateTime.UtcNow;
    }
}