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
    void IncrementTotalGetAndPutRequestCount();
    public void IncrementSuccessfulGetHitCount();
    void IncrementMissedGetCount();
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

    private long _totalRequests; // Calls to GET + PUT
    private long _cacheHits;     // Calls to GET when item is found
    private long _cacheMisses;   // Calls to GET when item is not found
    private long _evictionCount; // Evicted due to cache memory or count restrictions
    private long _expiredCount;  // Evicted due to TTL configuration
    private long _itemCount;     // Current cache item count
    private long _totalMemory;   // Current cache memory size

    public void IncrementTotalGetAndPutRequestCount() => Interlocked.Increment(ref _totalRequests);
    public long TotalRequests => _totalRequests;

    public void IncrementSuccessfulGetHitCount() => Interlocked.Increment(ref _cacheHits);
    public long CacheHits => _cacheHits;

    public void IncrementMissedGetCount() => Interlocked.Increment(ref _cacheMisses);
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