using EnhancedLRUCache;
using EnhancedLRUCache.Errors;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Setting up cache with various configurations...\n");

        var policy = new LruPolicy(TtlPolicy.Absolute | TtlPolicy.Sliding, TimeSpan.FromSeconds(10));
        var stats = new CacheStats();
        var storage = new LruStorage<string, string>(
            maxItemCount: 5,
            maxMemorySize: 1024 * 1024, // 1MB
            stats: stats
        );

        var cache = new LruCache<string, string>(
            storage: storage,
            policy: policy,
            stats: stats,
            lockTimeout: TimeSpan.FromSeconds(30),
            cleanupInterval: TimeSpan.FromSeconds(5),
            cleanupRetryInterval: TimeSpan.FromSeconds(1)
        );

        // Subscribe to events
        cache.ItemExpired += (sender, e) => Console.WriteLine($"Item expired - Key: {e.Key}, Value: {e.Value}, Time: {e.Timestamp}");
        cache.ItemEvicted += (sender, e) => Console.WriteLine($"Item evicted - Key: {e.Key}, Value: {e.Value}, Time: {e.Timestamp}");

        try
        {
            // Test 1: Basic Operations
            Console.WriteLine("Test 1: Basic Operations");
            await TestBasicOperations(cache);

            // Test 2: Expiration
            Console.WriteLine("\nTest 2: Expiration");
            await TestExpiration(cache);

            // Test 3: LRU Eviction
            Console.WriteLine("\nTest 3: LRU Eviction");
            await TestLRUEviction(cache);

            // Test 4: Concurrent Operations
            Console.WriteLine("\nTest 4: Concurrent Operations");
            await TestConcurrentOperations(cache);

            // Test 5: Statistics
            Console.WriteLine("\nTest 5: Statistics");
            PrintStatistics(stats);
        }
        finally
        {
            cache.Dispose();
        }
    }

    static async Task TestBasicOperations(ILruCache<string, string> cache)
    {
        // Put
        bool putSuccess = cache.Put("key1", "value1", TimeSpan.FromMinutes(1), out var putError);
        Console.WriteLine($"Put key1: {putSuccess}, Error: {putError}");

        // Get
        bool getSuccess = cache.TryGet("key1", out var getValue, out var getError);
        Console.WriteLine($"Get key1: {getSuccess}, Value: {getValue}, Error: {getError}");

        // Remove
        bool removeSuccess = cache.Remove("key1", out var removedValue, out var removeError);
        Console.WriteLine($"Remove key1: {removeSuccess}, Value: {removedValue}, Error: {removeError}");

        // Get non-existent
        bool getNonExistentSuccess = cache.TryGet("key1", out var nonExistentValue, out var getNonExistentError);
        Console.WriteLine($"Get after remove: {getNonExistentSuccess}, Error: {getNonExistentError}");
    }

    static async Task TestExpiration(ILruCache<string, string> cache)
    {
        // Add item with short TTL
        bool putSuccess = cache.Put("shortTTL", "This will expire soon", TimeSpan.FromSeconds(2), out var putError);
        Console.WriteLine($"Added item with short TTL: {putSuccess}, Error: {putError}");

        // Verify it exists
        bool getSuccess = cache.TryGet("shortTTL", out var getValue, out var getError);
        Console.WriteLine($"Item found before expiration: {getSuccess}, Value: {getValue}, Error: {getError}");

        // Wait for expiration
        await Task.Delay(2500); // Wait for 2.5 seconds

        // Try to get expired item
        bool getExpiredSuccess = cache.TryGet("shortTTL", out var expiredValue, out var getExpiredError);
        Console.WriteLine($"Item found after expiration: {getExpiredSuccess}, Error: {getExpiredError}");
    }

    static async Task TestLRUEviction(ILruCache<string, string> cache)
    {
        // Clear cache first
        bool clearSuccess = cache.Clear();
        Console.WriteLine($"Cache cleared: {clearSuccess}");

        // Add items up to capacity (5 items)
        for (int i = 1; i <= 5; i++)
        {
            bool putSuccess = cache.Put($"key{i}", $"value{i}", TimeSpan.FromMinutes(1), out var putError);
            Console.WriteLine($"Added key{i}: {putSuccess}, Error: {putError}");
        }

        // Access key2 and key3 to make them more recently used
        cache.TryGet("key2", out _, out var getError2);
        cache.TryGet("key3", out _, out var getError3);

        // Add new item to trigger eviction of least recently used
        bool putEvictSuccess = cache.Put("key6", "value6", TimeSpan.FromMinutes(1), out var putEvictError);
        Console.WriteLine($"Added key6: {putEvictSuccess}, Error: {putEvictError}");

        // Try to get key1 (should be evicted)
        bool getEvictedSuccess = cache.TryGet("key1", out var evictedValue, out var getEvictedError);
        Console.WriteLine($"Try get evicted key1: {getEvictedSuccess}, Error: {getEvictedError}");
    }

    static async Task TestConcurrentOperations(ILruCache<string, string> cache)
    {
        var tasks = new List<Task>();
        var random = new Random();
        var operationCount = new TaskCompletionSource();

        // Create multiple tasks for concurrent operations
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < 5; j++)
                {
                    string key = $"concurrent_key_{random.Next(1, 10)}";
                    string value = $"value_{DateTime.UtcNow.Ticks}";

                    // Random operations: put, get, or remove
                    switch (random.Next(3))
                    {
                        case 0:
                            bool putSuccess = cache.Put(key, value, TimeSpan.FromSeconds(30), out var putError);
                            if (!putSuccess)
                            {
                                Console.WriteLine($"Concurrent put failed for {key}: {putError}");
                            }

                            break;

                        case 1:
                            bool getSuccess = cache.TryGet(key, out var getValue, out var getError);
                            if (!getSuccess && getError != CacheRetrievalError.ItemNotFound)
                            {
                                Console.WriteLine($"Concurrent get failed for {key}: {getError}");
                            }

                            break;

                        case 2:
                            bool removeSuccess = cache.Remove(key, out var removedValue, out var removeError);
                            if (!removeSuccess && removeError != CacheRemovalError.ItemNotFound)
                            {
                                Console.WriteLine($"Concurrent remove failed for {key}: {removeError}");
                            }

                            break;
                    }

                    await Task.Delay(100); // Small delay to simulate work
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("Completed concurrent operations test");
    }

    static void PrintStatistics(ICacheStats stats)
    {
        Console.WriteLine($"Total Requests: {stats.TotalRequests}");
        Console.WriteLine($"Cache Hits: {stats.CacheHits}");
        Console.WriteLine($"Cache Misses: {stats.CacheMisses}");
        Console.WriteLine($"Hit Ratio: {stats.HitRatio:P2}");
        Console.WriteLine($"Eviction Count: {stats.EvictionCount}");
        Console.WriteLine($"Expired Count: {stats.ExpiredCount}");
        Console.WriteLine($"Current Item Count: {stats.CurrentItemCount}");
        Console.WriteLine($"Total Memory: {stats.TotalMemoryBytes:N0} bytes");
    }
}