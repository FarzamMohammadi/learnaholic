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

return;

static string KeyFromNumber(int number) => new($"key{number}");
static string ValueFromNumber(int number) => new($"value{number}");

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
        maxMemorySize: 1024 * 1024
    );

    await Task.Yield(); // Make test awaitable

    var basicPutSuccessful = cache.Put(KeyFromNumber(1), ValueFromNumber(1), null, out var error);
    if (basicPutSuccessful && error != CacheAdditionError.None) return false;

    var basicGetIsSuccessful = cache.TryGet(KeyFromNumber(1), out var value, out var retrievalError);
    if (basicGetIsSuccessful && retrievalError != CacheRetrievalError.None || value != ValueFromNumber(1)) return false;

    var removeIsSuccessful = cache.Remove(KeyFromNumber(1), out _, out var removalError);
    if (removeIsSuccessful && removalError != CacheRemovalError.None) return false;

    var removedItemIsFound = cache.TryGet(KeyFromNumber(1), out _, out retrievalError);
    return !removedItemIsFound && retrievalError == CacheRetrievalError.ItemNotFound;
}

async Task<bool> MemoryLimitsTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 100
    );

    await Task.Yield(); // Make test awaitable

    var smallCacheItemInserted = cache.Put("small", "x", null, out var smallItemError);
    if (!smallCacheItemInserted || smallItemError != CacheAdditionError.None) return false;

    var largerThanMemoryLimitCacheItemInserted = new string('x', 1000);
    var success2 = cache.Put("large", largerThanMemoryLimitCacheItemInserted, null, out var largeItemError);

    return !success2 && largeItemError == CacheAdditionError.MaxMemorySizeExceeded;
}

async Task<bool> ItemCountLimitsTest()
{
    var cache = new Cache<int, string>(
        maxItemCount: 3,
        maxMemorySize: 1024 * 1024
    );

    await Task.Yield(); // Make test awaitable

    for (var i = 0; i < 3; i++)
    {
        cache.Put(i, $"value{i}", null, out _);
    }

    cache.Put(3, "value3", null, out _);

    var evictionCandidateWasFound = cache.TryGet(0, out _, out var error);
    if (!evictionCandidateWasFound && error != CacheRetrievalError.ItemNotFound) return false;

    var allItemsExceptForEvictedEntryExist = cache.TryGet(1, out _, out _) &&
                                             cache.TryGet(2, out _, out _) &&
                                             cache.TryGet(3, out _, out _);

    return allItemsExceptForEvictedEntryExist;
}

async Task<bool> StatisticsTrackingTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024
    );

    await Task.Yield(); // Make test awaitable

    // Increase total req count
    cache.Put(KeyFromNumber(1), ValueFromNumber(1), TimeSpan.FromMinutes(5), out _);
    cache.Put(KeyFromNumber(2), ValueFromNumber(2), TimeSpan.FromMinutes(5), out _);

    // Increase hit & total req count
    cache.TryGet(KeyFromNumber(1), out _, out _);
    cache.TryGet(KeyFromNumber(2), out _, out _);

    // Increase miss count
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
        maxMemorySize: 100
    );

    await Task.Yield(); // Make test awaitable

    // Test null key handling
    try
    {
        cache.Put(null!, "value", null, out _);
        return false; // Should not reach this line
    }
    catch (ArgumentNullException)
    {
    }

    var insertedCacheItemThatExceedsMemorySize = cache.Put("key", new string('x', 1000), null, out var error);
    if (insertedCacheItemThatExceedsMemorySize || error != CacheAdditionError.MaxMemorySizeExceeded) return false;

    var foundNonExistingItem = cache.TryGet("nonexistent", out _, out var retrievalError);
    return !foundNonExistingItem && retrievalError == CacheRetrievalError.ItemNotFound;
}

async Task<bool> AbsoluteExpirationTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Absolute)
    );

    cache.Put(KeyFromNumber(1), ValueFromNumber(1), TimeSpan.FromSeconds(1), out _);

    var foundItem = cache.TryGet(KeyFromNumber(1), out var value, out _);

    if (!foundItem || value != ValueFromNumber(1)) return false;

    await ItemExpiration();

    var expiredItemFound = cache.TryGet(KeyFromNumber(1), out _, out var error);
    return !expiredItemFound && error == CacheRetrievalError.ItemExpired;

    async Task ItemExpiration() => await Task.Delay(1500);
}

async Task<bool> SlidingExpirationTest()
{
    var cache = new Cache<string, string>(
        maxItemCount: 100,
        maxMemorySize: 1024 * 1024,
        policy: new EvictionPolicy(TtlPolicy.Sliding)
    );

    // Add item with 2 second sliding window
    cache.Put(KeyFromNumber(1), ValueFromNumber(1), TimeSpan.FromSeconds(2), out _);

    // Access item after 1 second (should reset timer)
    await Task.Delay(TimeSpan.FromSeconds(1));
    if (!cache.TryGet(KeyFromNumber(1), out _, out _)) return false;

    // Access item after another 1 second (should still exist)
    await Task.Delay(TimeSpan.FromSeconds(1));
    if (!cache.TryGet(KeyFromNumber(1), out _, out _)) return false;

    // Wait for full expiration without access
    await Task.Delay(TimeSpan.FromSeconds(2));
    cache.TryGet(KeyFromNumber(1), out _, out var error);

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

    cache.ItemEvicted += (_, _) => evictionCount++;
    cache.ItemExpired += (_, _) => expirationCount++;

    // Add items to trigger eviction
    cache.Put(KeyFromNumber(1), ValueFromNumber(1), TimeSpan.FromMinutes(5), out _);
    cache.Put(KeyFromNumber(2), ValueFromNumber(2), TimeSpan.FromMinutes(5), out _);
    cache.Put(KeyFromNumber(3), ValueFromNumber(3), TimeSpan.FromMinutes(5), out _); // Trigger ItemEvicted (for key1)

    // Add item with short TTL to trigger expiration
    cache.Put("expiring", "value", TimeSpan.FromSeconds(1), out _); // Trigger ItemEvicted (for key2)
    await Task.Delay(TimeSpan.FromSeconds(2));
    cache.TryGet("expiring", out _, out _); // Trigger itemExpired

    return evictionCount == 2 && expirationCount == 1;
}

async Task<bool> ConcurrentOperationsTest()
{
    var cache = new Cache<int, string>(
        maxItemCount: 1000,
        maxMemorySize: 1024 * 1024
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
    await Task.Delay(TimeSpan.FromSeconds(3));

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

    var allExpiredItemsWereRemoved = cache.Metrics.CurrentItemCount == 0;

    return allExpiredItemsWereRemoved;
}