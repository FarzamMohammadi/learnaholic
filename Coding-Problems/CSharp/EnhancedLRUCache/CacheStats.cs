namespace EnhancedLRUCache;

public interface ICacheStats
{
    public void IncrementRequestCount();
    long TotalRequests { get; }

    long CacheHits => TotalRequests - CacheMisses;

    public void IncrementMissedRequestCount();
    long CacheMisses { get; }

    double HitRatio => TotalRequests == 0 ? 0 : (double)CacheHits / TotalRequests;

    public void IncrementEvictionCount();
    long EvictionCount { get; }

    public void IncrementExpiredCount();
    long ExpiredCount { get; }

    public void UpdateItemCount(int count);
    long CurrentItemCount { get; }

    public void UpdateMemory(long size);
    long TotalMemoryBytes { get; }

    public void ClearMetrics();
}

public class CacheStats : ICacheStats
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

    public void IncrementMissedRequestCount() => Interlocked.Increment(ref _cacheMisses);
    public long CacheMisses => _cacheMisses;

    public void IncrementEvictionCount() => Interlocked.Increment(ref _evictionCount);
    public long EvictionCount => _evictionCount;

    public void IncrementExpiredCount() => Interlocked.Increment(ref _expiredCount);
    public long ExpiredCount => _expiredCount;

    public void UpdateItemCount(int count) => Interlocked.Add(ref _itemCount, count);
    public long CurrentItemCount => _itemCount;

    public void UpdateMemory(long size) => Interlocked.Add(ref _totalMemory, size);
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
}