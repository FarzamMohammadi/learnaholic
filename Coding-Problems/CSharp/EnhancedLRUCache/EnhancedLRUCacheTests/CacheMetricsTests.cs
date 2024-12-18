using EnhancedLRUCache.Caching.Monitoring;
using Xunit;

namespace EnhancedLRUCacheTests;

public class CacheMetricsTests
{
    private readonly CacheMetrics _metrics = new();

    [Fact]
    public void BasicCounterIncrements_ShouldWork()
    {
        _metrics.IncrementRequestCount();
        _metrics.IncrementMissedRequestCount();
        _metrics.IncrementEvictionCount();
        _metrics.IncrementExpiredCount();

        Assert.Equal(1, _metrics.TotalRequests);
        Assert.Equal(1, _metrics.CacheMisses);
        Assert.Equal(1, _metrics.EvictionCount);
        Assert.Equal(1, _metrics.ExpiredCount);
    }

    [Fact]
    public void CacheHits_ShouldCalculateCorrectly()
    {
        // Simulating a scenario with 60% hit rate
        for (var i = 0; i < 5; i++)
        {
            _metrics.IncrementRequestCount();
        }

        _metrics.IncrementMissedRequestCount();
        _metrics.IncrementMissedRequestCount();

        Assert.Equal(3, _metrics.CacheHits);
        Assert.Equal(2, _metrics.CacheMisses);
        Assert.Equal(5, _metrics.TotalRequests);
    }

    [Fact]
    public void HitRatio_ShouldCalculateCorrectly()
    {
        // Verifying edge case when no requests have been made
        Assert.Equal(0, _metrics.HitRatio);

        // Testing hit ratio with 75% cache hits (3 hits out of 4 requests)
        for (var i = 0; i < 4; i++)
        {
            _metrics.IncrementRequestCount();
        }

        _metrics.IncrementMissedRequestCount();

        Assert.Equal(0.75, _metrics.HitRatio);
    }

    [Fact]
    public void UpdateOperations_ShouldWork()
    {
        // Testing both positive and negative deltas for item count and memory updates
        _metrics.AddNewItem(5);
        Assert.Equal(1, _metrics.CurrentItemCount);
        Assert.Equal(5, _metrics.TotalMemoryBytes);

        _metrics.AddNewItem(1024);
        Assert.Equal(2, _metrics.CurrentItemCount);
        Assert.Equal(1024 + 5, _metrics.TotalMemoryBytes);

        _metrics.RemoveItem(1024);
        Assert.Equal(1, _metrics.CurrentItemCount);
        Assert.Equal(5, _metrics.TotalMemoryBytes);

        _metrics.RemoveItem(5);
        Assert.Equal(0, _metrics.TotalMemoryBytes);
        Assert.Equal(0, _metrics.TotalMemoryBytes);
    }

    [Fact]
    public void ClearMetrics_ShouldResetAllCounters()
    {
        // Populating metrics before reset
        _metrics.IncrementRequestCount();
        _metrics.IncrementMissedRequestCount();
        _metrics.IncrementEvictionCount();
        _metrics.IncrementExpiredCount();

        foreach (var i in Enumerable.Range(1, 10)) _metrics.AddNewItem(10);

        _metrics.ClearMetrics();

        // Verifying complete reset of all metrics
        Assert.Equal(0, _metrics.TotalRequests);
        Assert.Equal(0, _metrics.CacheMisses);
        Assert.Equal(0, _metrics.CacheHits);
        Assert.Equal(0, _metrics.EvictionCount);
        Assert.Equal(0, _metrics.ExpiredCount);
        Assert.Equal(0, _metrics.CurrentItemCount);
        Assert.Equal(0, _metrics.TotalMemoryBytes);
        Assert.Equal(0, _metrics.HitRatio);
    }

    [Fact]
    public async Task ThreadSafety_ShouldHandleConcurrentOperations()
    {
        // Testing thread safety by running multiple operations concurrently
        // This helps identify potential race conditions in the atomic operations
        const int iterations = 1000;
        var tasks = new Task[3];

        tasks[0] = Task.Run(() =>
        {
            for (var i = 0; i < iterations; i++)
                _metrics.IncrementRequestCount();
        });

        tasks[1] = Task.Run(() =>
        {
            for (var i = 0; i < iterations; i++)
                _metrics.IncrementMissedRequestCount();
        });

        tasks[2] = Task.Run(() =>
        {
            for (var i = 0; i < iterations; i++) _metrics.AddNewItem(1);
        });

        await Task.WhenAll(tasks);

        Assert.Equal(iterations, _metrics.TotalRequests);
        Assert.Equal(iterations, _metrics.CacheMisses);
        Assert.Equal(iterations, _metrics.TotalMemoryBytes);
        Assert.Equal(iterations, _metrics.CurrentItemCount);
    }
}