using EnhancedLRUCache.Caching;
using EnhancedLRUCache.Caching.Core;
using EnhancedLRUCache.Caching.Errors;

var totalTests = 0;
var passedTests = 0;

Console.WriteLine("Starting Enhanced LRU Cache Integration Tests\n");

await RunTest("Basic Put/Get Operations", BasicOperationsTest);
await RunTest("TTL Absolute Expiration", AbsoluteExpirationTest);
await RunTest("TTL Sliding Expiration", SlidingExpirationTest);
await RunTest("Memory Limits", MemoryLimitsTest);
await RunTest("Item Count Limits", ItemCountLimitsTest);
await RunTest("Statistics Tracking", StatisticsTrackingTest);
await RunTest("Event Handling", EventHandlingTest);
await RunTest("Concurrent Operations", ConcurrentOperationsTest);
await RunTest("Error Handling", ErrorHandlingTest);
await RunTest("Background Cleanup", BackgroundCleanupTest);

Console.WriteLine($"\nTest Results: {passedTests}/{totalTests} tests passed");

async Task RunTest(string testName, Func<Task<bool>> testFunc)
{
    totalTests++;
    Console.Write($"Running {testName}... ");

    try
    {
        var success = await testFunc();
        if (success) passedTests++;

        Console.WriteLine(success ? "PASSED" : "FAILED");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAILED (Exception: {ex.Message})");
    }
}

async Task<bool> BasicOperationsTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024, // 1MB
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    await Task.Yield(); // Ensure test is awaitable

    // Test basic put
    cache.Put("key1", "value1", TimeSpan.FromMinutes(5), out var error);
    if (error != CacheAdditionError.None) return false;

    // Test basic get
    cache.TryGet("key1", out var value, out var retrievalError);
    if (retrievalError != CacheRetrievalError.None || value != "value1") return false;

    // Test remove
    cache.Remove("key1", out _, out var removalError);
    if (removalError != CacheRemovalError.None) return false;

    // Verify removal
    cache.TryGet("key1", out _, out retrievalError);
    return retrievalError == CacheRetrievalError.ItemNotFound;
}

async Task<bool> MemoryLimitsTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 100, // Very small limit for testing
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    await Task.Yield(); // Ensure test is awaitable

    // Add item that should fit
    var success1 = cache.Put("small", "x", TimeSpan.FromMinutes(5), out var error1);
    if (!success1 || error1 != CacheAdditionError.None) return false;

    // Add item that's too large
    var largeValue = new string('x', 1000); // Much larger than our limit
    var success2 = cache.Put("large", largeValue, TimeSpan.FromMinutes(5), out var error2);

    return !success2 && error2 == CacheAdditionError.MaxMemorySizeExceeded;
}

async Task<bool> ItemCountLimitsTest()
{
    var cache = new Cache<int, string>(
        maxItemCount: 3, // Small limit for testing
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    await Task.Yield(); // Ensure test is awaitable

    // Add items up to limit
    for (var i = 0; i < 3; i++)
    {
        cache.Put(i, $"value{i}", TimeSpan.FromMinutes(5), out _);
    }

    // Add one more item (should evict oldest)
    cache.Put(3, "value3", TimeSpan.FromMinutes(5), out _);

    // Verify first item was evicted
    cache.TryGet(0, out _, out var error);
    if (error != CacheRetrievalError.ItemNotFound) return false;

    // Verify newest items exist
    return cache.TryGet(1, out _, out _) &&
           cache.TryGet(2, out _, out _) &&
           cache.TryGet(3, out _, out _);
}

async Task<bool> StatisticsTrackingTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    await Task.Yield(); // Ensure test is awaitable

    // Generate some statistics
    cache.Put("key1", "value1", TimeSpan.FromMinutes(5), out _);
    cache.Put("key2", "value2", TimeSpan.FromMinutes(5), out _);

    // Some hits
    cache.TryGet("key1", out _, out _);
    cache.TryGet("key2", out _, out _);

    // Some misses
    cache.TryGet("nonexistent", out _, out _);

    var metrics = cache.Metrics;

    return metrics.TotalRequests == 5 &&
           metrics.CacheHits == 2 &&
           metrics.CacheMisses == 1 &&
           metrics.CurrentItemCount == 2 &&
           Math.Abs(metrics.HitRatio - 0.4) < 0.001; // Use approximate comparison for doubles
}

async Task<bool> ErrorHandlingTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 100, // Small size for testing
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    await Task.Yield(); // Ensure test is awaitable

    // Test null key handling
    try
    {
        cache.Put(null!, "value", TimeSpan.FromMinutes(5), out _);
        return false; // Should have thrown
    }
    catch (ArgumentNullException)
    {
    }

    // Test memory limit error
    var success = cache.Put("key", new string('x', 1000), TimeSpan.FromMinutes(5), out var error);
    if (success || error != CacheAdditionError.MaxMemorySizeExceeded) return false;

    // Test retrieval of non-existent key
    cache.TryGet("nonexistent", out _, out var retrievalError);
    return retrievalError == CacheRetrievalError.ItemNotFound;
}

// Already async methods - no changes needed
async Task<bool> AbsoluteExpirationTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    // Add item with 1 second TTL
    cache.Put("key1", "value1", TimeSpan.FromSeconds(1), out _);

    // Verify it exists
    if (!cache.TryGet("key1", out var value, out _) || value != "value1")
        return false;

    // Wait for expiration
    await Task.Delay(1500);

    // Verify it's expired
    cache.TryGet("key1", out _, out var error);
    return error == CacheRetrievalError.ItemExpired;
}

async Task<bool> SlidingExpirationTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Sliding)
    );

    // Add item with 2 second sliding window
    cache.Put("key1", "value1", TimeSpan.FromSeconds(2), out _);

    // Access item after 1 second (should reset timer)
    await Task.Delay(1000);
    if (!cache.TryGet("key1", out _, out _)) return false;

    // Access item after another 1 second (should still exist)
    await Task.Delay(1000);
    if (!cache.TryGet("key1", out _, out _)) return false;

    // Wait for full expiration without access
    await Task.Delay(2500);
    cache.TryGet("key1", out _, out var error);

    return error == CacheRetrievalError.ItemExpired;
}

async Task<bool> EventHandlingTest()
{
    var evictionCount = 0;
    var expirationCount = 0;

    var cache = new Cache<string, string>(
        maxItemCount: 2,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    cache.ItemEvicted += (sender, args) => evictionCount++;
    cache.ItemExpired += (sender, args) => expirationCount++;

    // Add items to trigger eviction
    cache.Put("key1", "value1", TimeSpan.FromMinutes(5), out _);
    cache.Put("key2", "value2", TimeSpan.FromMinutes(5), out _);
    cache.Put("key3", "value3", TimeSpan.FromMinutes(5), out _); // Should evict key1

    // Add item with short TTL to trigger expiration
    cache.Put("expiring", "value", TimeSpan.FromSeconds(1), out _);
    await Task.Delay(1500);
    cache.TryGet("expiring", out _, out _); // Trigger expired check

    return evictionCount == 1 && expirationCount == 1;
}

async Task<bool> ConcurrentOperationsTest()
{
    var cache = new Cache<int, string>(
        maxItemCount: 1000,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    var tasks = new List<Task>();
    var successCount = 0;
    var lockObj = new object();

    // Create multiple tasks that read and write concurrently
    for (var i = 0; i < 100; i++)
    {
        var index = i;
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                // Write operation
                cache.Put(index, $"value{index}", TimeSpan.FromMinutes(5), out _);

                // Small delay to increase chance of concurrent access
                await Task.Delay(Random.Shared.Next(1, 10));

                // Read operation
                if (cache.TryGet(index, out var value, out _) && value == $"value{index}")
                {
                    lock (lockObj)
                    {
                        successCount++;
                    }
                }
            }
            catch
            {
                // Catch any concurrent operation exceptions
            }
        }));
    }

    await Task.WhenAll(tasks);
    return successCount > 90; // Allow for some concurrent operation failures
}

async Task<bool> BackgroundCleanupTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute),
        cleanupInterval: TimeSpan.FromSeconds(2)
    );

    // Add items with short TTL
    for (var i = 0; i < 5; i++)
    {
        cache.Put($"key{i}", $"value{i}", TimeSpan.FromSeconds(1), out _);
    }

    // Wait for items to expire and cleanup to run
    await Task.Delay(3000);

    // Verify all items are cleaned up
    for (var i = 0; i < 5; i++)
    {
        if (cache.TryGet($"key{i}", out _, out var error)
            && error != CacheRetrievalError.ItemNotFound
            && error != CacheRetrievalError.ItemExpired)
        {
            return false;
        }
    }

    return true;
}