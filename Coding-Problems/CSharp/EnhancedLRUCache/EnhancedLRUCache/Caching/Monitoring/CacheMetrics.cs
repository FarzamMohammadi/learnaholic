namespace EnhancedLRUCache.Caching.Monitoring;

public interface ICacheMetrics
{
    long TotalRequests { get; }
    long CacheHits { get; }
    long CacheMisses { get; }
    double HitRatio { get; }
    long EvictionCount { get; }
    long ExpiredCount { get; }
    long CurrentItemCount { get; }
    long TotalMemoryBytes { get; }
}

internal interface ICacheMetricsInternal : ICacheMetrics
{
    void IncrementRequestCount();
    void IncrementMissedRequestCount();
    void IncrementEvictionCount();
    void IncrementExpiredCount();
    void ClearMetrics();
    void AddNewItem(long size);
    void RemoveItem(long size);
}

public class CacheMetrics : ICacheMetricsInternal
{
    // The Interlocked class ensures atomic operations for thread-safe counting
    // https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked?view=net-9.0

    private long _totalRequests;
    private long _cacheMisses;
    private long _evictionCount;
    private long _expiredCount;
    private long _itemCount;
    private long _totalMemory;

    public void IncrementRequestCount() => Interlocked.Increment(ref _totalRequests);
    public long TotalRequests => _totalRequests;

    public long CacheHits
    {
        get
        {
            var requests = TotalRequests;
            var misses = CacheMisses;
            return requests - misses;
        }
    }

    public void IncrementMissedRequestCount() => Interlocked.Increment(ref _cacheMisses);
    public long CacheMisses => _cacheMisses;

    public double HitRatio
    {
        get
        {
            var requests = TotalRequests;
            return requests == 0 ? 0 : (double)CacheHits / requests;
        }
    }

    public void IncrementEvictionCount() => Interlocked.Increment(ref _evictionCount);
    public long EvictionCount => _evictionCount;

    public void IncrementExpiredCount() => Interlocked.Increment(ref _expiredCount);
    public long ExpiredCount => _expiredCount;

    public long CurrentItemCount => _itemCount;

    public long TotalMemoryBytes => _totalMemory;

    public void ClearMetrics()
    {
        Interlocked.Exchange(ref _totalRequests, 0);
        Interlocked.Exchange(ref _cacheMisses, 0);
        Interlocked.Exchange(ref _evictionCount, 0);
        Interlocked.Exchange(ref _expiredCount, 0);
        Interlocked.Exchange(ref _itemCount, 0);
        Interlocked.Exchange(ref _totalMemory, 0);
    }

    public void AddNewItem(long size)
    {
        Interlocked.Increment(ref _itemCount);
        Interlocked.Add(ref _totalMemory, size);
    }

    public void RemoveItem(long size)
    {
        Interlocked.Decrement(ref _itemCount);
        Interlocked.Add(ref _totalMemory, -1 * size);
    }
}