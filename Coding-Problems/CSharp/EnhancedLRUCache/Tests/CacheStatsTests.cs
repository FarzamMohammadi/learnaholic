using Xunit;

namespace EnhancedLRUCache.Tests;

public class CacheStatsTests
{
    private readonly CacheStats _stats = new();

    [Fact]
    public void BasicCounterIncrements_ShouldWork()
    {
        _stats.IncrementRequestCount();
        _stats.IncrementMissedRequestCount();
        _stats.IncrementEvictionCount();
        _stats.IncrementExpiredCount();

        Assert.Equal(1, _stats.TotalRequests);
        Assert.Equal(1, _stats.CacheMisses);
        Assert.Equal(1, _stats.EvictionCount);
        Assert.Equal(1, _stats.ExpiredCount);
    }

    [Fact]
    public void CacheHits_ShouldCalculateCorrectly()
    {
        // Simulating a scenario with 60% hit rate
        for (int i = 0; i < 5; i++)
        {
            _stats.IncrementRequestCount();
        }

        _stats.IncrementMissedRequestCount();
        _stats.IncrementMissedRequestCount();

        Assert.Equal(3, _stats.CacheHits);
        Assert.Equal(2, _stats.CacheMisses);
        Assert.Equal(5, _stats.TotalRequests);
    }

    [Fact]
    public void HitRatio_ShouldCalculateCorrectly()
    {
        // Verifying edge case when no requests have been made
        Assert.Equal(0, _stats.HitRatio);

        // Testing hit ratio with 75% cache hits (3 hits out of 4 requests)
        for (int i = 0; i < 4; i++)
        {
            _stats.IncrementRequestCount();
        }

        _stats.IncrementMissedRequestCount();

        Assert.Equal(0.75, _stats.HitRatio);
    }

    [Fact]
    public void UpdateOperations_ShouldWork()
    {
        // Testing both positive and negative deltas for item count and memory updates
        _stats.UpdateItemCount(5);
        Assert.Equal(5, _stats.CurrentItemCount);

        _stats.UpdateMemory(1024);
        Assert.Equal(1024, _stats.TotalMemoryBytes);

        _stats.UpdateItemCount(-2);
        Assert.Equal(3, _stats.CurrentItemCount);

        _stats.UpdateMemory(-512);
        Assert.Equal(512, _stats.TotalMemoryBytes);
    }

    [Fact]
    public void ClearMetrics_ShouldResetAllCounters()
    {
        // Populating metrics before reset
        _stats.IncrementRequestCount();
        _stats.IncrementMissedRequestCount();
        _stats.IncrementEvictionCount();
        _stats.IncrementExpiredCount();
        _stats.UpdateItemCount(5);
        _stats.UpdateMemory(1024);

        _stats.ClearMetrics();

        // Verifying complete reset of all metrics
        Assert.Equal(0, _stats.TotalRequests);
        Assert.Equal(0, _stats.CacheMisses);
        Assert.Equal(0, _stats.CacheHits);
        Assert.Equal(0, _stats.EvictionCount);
        Assert.Equal(0, _stats.ExpiredCount);
        Assert.Equal(0, _stats.CurrentItemCount);
        Assert.Equal(0, _stats.TotalMemoryBytes);
        Assert.Equal(0, _stats.HitRatio);
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
            for (int i = 0; i < iterations; i++)
                _stats.IncrementRequestCount();
        });

        tasks[1] = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
                _stats.IncrementMissedRequestCount();
        });

        tasks[2] = Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
                _stats.UpdateMemory(1);
        });

        await Task.WhenAll(tasks);

        Assert.Equal(iterations, _stats.TotalRequests);
        Assert.Equal(iterations, _stats.CacheMisses);
        Assert.Equal(iterations, _stats.TotalMemoryBytes);
    }
}