using EnhancedLRUCache;
using EnhancedLRUCache.Errors;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Enhanced LRU Cache Integration Tests ===\n");

        // Test string cache (most common use case)
        await Console.Out.WriteLineAsync("\n=== Testing String Cache ===");
        await TestStringCache();

        // Test complex object cache
        await Console.Out.WriteLineAsync("\n=== Testing Complex Object Cache ===");
        await TestComplexObjectCache();

        // Test primitive type cache
        await Console.Out.WriteLineAsync("\n=== Testing Integer Cache ===");
        await TestIntegerCache();
    }

    static async Task TestStringCache()
    {
        var stats = new CacheStats();
        var storage = new LruStorage<string, string>(
            maxItemCount: 1000,
            maxMemorySize: 10 * 1024 * 1024,
            stats: stats
        );

        await TestWithPolicy(storage, stats, "Absolute TTL Policy", TtlPolicy.Absolute);
        await TestWithPolicy(storage, stats, "Sliding TTL Policy", TtlPolicy.Sliding);
        await TestWithPolicy(storage, stats, "Combined TTL Policy", TtlPolicy.Absolute | TtlPolicy.Sliding);
    }

    static async Task TestComplexObjectCache()
    {
        var stats = new CacheStats();
        var storage = new LruStorage<string, TestObject>(
            maxItemCount: 100,
            maxMemorySize: 1 * 1024 * 1024,
            stats: stats
        );

        var policy = new LruPolicy(TtlPolicy.Absolute, TimeSpan.FromMinutes(5));
        using var cache = new LruCache<string, TestObject>(
            storage: storage,
            policy: policy,
            stats: stats,
            lockTimeout: TimeSpan.FromSeconds(30),
            cleanupInterval: TimeSpan.FromSeconds(5)
        );

        Console.WriteLine("Testing complex object operations...");

        // Test object storage and retrieval
        var obj1 = new TestObject { Id = 1, Name = "Test 1", Data = new byte[1024] };
        var obj2 = new TestObject { Id = 2, Name = "Test 2", Data = new byte[2048] };

        cache.Put("obj1", obj1, TimeSpan.FromMinutes(1), out _);
        cache.Put("obj2", obj2, TimeSpan.FromMinutes(1), out _);

        // Test object retrieval
        if (cache.TryGet("obj1", out var retrievedObj, out _))
        {
            Console.WriteLine($"Retrieved object: Id={retrievedObj.Id}, Name={retrievedObj.Name}, DataSize={retrievedObj.Data.Length}");
        }

        // Test concurrent object operations
        await TestConcurrentObjectOperations(cache);

        PrintStatistics(stats);
    }

    static async Task TestIntegerCache()
    {
        var stats = new CacheStats();
        var storage = new LruStorage<int, int>(
            maxItemCount: 100,
            maxMemorySize: 1024 * 1024,
            stats: stats
        );

        var policy = new LruPolicy(TtlPolicy.Sliding, TimeSpan.FromMinutes(1));
        using var cache = new LruCache<int, int>(
            storage: storage,
            policy: policy,
            stats: stats,
            lockTimeout: TimeSpan.FromSeconds(30)
        );

        Console.WriteLine("Testing integer operations...");

        // Test basic integer operations
        for (int i = 0; i < 50; i++)
        {
            cache.Put(i, i * i, TimeSpan.FromMinutes(1), out _);
        }

        // Test integer retrieval and computation
        if (cache.TryGet(7, out var square, out _))
        {
            Console.WriteLine($"7 squared = {square}");
        }

        // Test concurrent integer operations
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var key = (taskId * 100) + j;
                    cache.Put(key, key * key, TimeSpan.FromMinutes(1), out _);
                    if (cache.TryGet(key, out var value, out _))
                    {
                        if (value != key * key)
                        {
                            Console.WriteLine($"Computation error: {key} * {key} != {value}");
                        }
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        PrintStatistics(stats);
    }

    static async Task TestWithPolicy(
        LruStorage<string, string> storage,
        CacheStats stats,
        string testName,
        TtlPolicy policyType)
    {
        Console.WriteLine($"\n=== Testing with {testName} ===");

        var policy = new LruPolicy(policyType, TimeSpan.FromMinutes(5));
        using var cache = new LruCache<string, string>(
            storage: storage,
            policy: policy,
            stats: stats,
            lockTimeout: TimeSpan.FromSeconds(30),
            cleanupInterval: TimeSpan.FromSeconds(5),
            cleanupRetryInterval: TimeSpan.FromSeconds(1)
        );

        // Setup event monitoring
        var evictedItems = new List<string>();
        var expiredItems = new List<string>();

        cache.ItemEvicted += (_, e) =>
        {
            evictedItems.Add(e.Key);
            Console.WriteLine($"[Event] Item evicted - Key: {e.Key}, Value: {e.Value}, Time: {e.Timestamp}");
        };

        cache.ItemExpired += (_, e) =>
        {
            expiredItems.Add(e.Key);
            Console.WriteLine($"[Event] Item expired - Key: {e.Key}, Value: {e.Value}, Time: {e.Timestamp}");
        };

        // Test 1: Basic Operations with Different TTLs
        Console.WriteLine("\n1. Testing Basic Operations with Different TTLs");
        await TestBasicOperations(cache);

        // Test 2: Memory Pressure and Eviction
        Console.WriteLine("\n2. Testing Memory Pressure and Eviction");
        await TestMemoryPressure(cache);

        // Test 3: Concurrent Access
        Console.WriteLine("\n3. Testing Concurrent Access");
        await TestConcurrentAccess(cache);

        // Test 4: TTL and Expiration
        Console.WriteLine("\n4. Testing TTL and Expiration");
        await TestTtlAndExpiration(cache);

        // Test 5: Edge Cases
        Console.WriteLine("\n5. Testing Edge Cases");
        await TestEdgeCases(cache);

        // Print final statistics
        Console.WriteLine($"\nFinal Statistics for {testName}:");
        PrintStatistics(stats);

        // Clear cache for next test
        cache.Clear();
    }

    static async Task TestBasicOperations(ILruCache<string, string> cache)
    {
        // Test different TTL durations
        var ttls = new[] 
        {
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromHours(1),
            TimeSpan.Zero
        };

        foreach (var ttl in ttls)
        {
            var key = $"basic_key_{ttl.TotalSeconds}";
            var value = $"value_for_{key}";

            bool putSuccess = cache.Put(key, value, ttl, out var putError);
            Console.WriteLine($"Put {key}: {putSuccess}, Error: {putError}");

            bool getSuccess = cache.TryGet(key, out var getValue, out var getError);
            Console.WriteLine($"Get {key}: {getSuccess}, Value: {(getValue == value)}, Error: {getError}");

            // Test update
            var newValue = $"updated_{value}";
            putSuccess = cache.Put(key, newValue, ttl, out putError);
            Console.WriteLine($"Update {key}: {putSuccess}, Error: {putError}");

            // Test removal
            bool removeSuccess = cache.Remove(key, out var removedValue, out var removeError);
            Console.WriteLine($"Remove {key}: {removeSuccess}, Value: {(removedValue == newValue)}, Error: {removeError}");
        }
    }

    static async Task TestMemoryPressure(ILruCache<string, string> cache)
    {
        var largeValue = new string('X', 1024 * 1024); // 1MB string
        Console.WriteLine("Adding large values to test memory pressure...");

        for (int i = 0; i < 15; i++) // Try to add more than memory limit
        {
            var key = $"large_key_{i}";
            bool success = cache.Put(key, largeValue, TimeSpan.FromMinutes(30), out var error);
            Console.WriteLine($"Put {key}: {success}, Error: {error}");

            if (i % 3 == 0) // Access every third item to affect LRU order
            {
                cache.TryGet(key, out _, out _);
            }
        }
    }

    static async Task TestConcurrentAccess(ILruCache<string, string> cache)
    {
        const int concurrentTasks = 50;
        const int operationsPerTask = 100;
        var random = new Random();
        var tasks = new List<Task>();

        Console.WriteLine($"Starting {concurrentTasks} tasks with {operationsPerTask} operations each...");

        for (int i = 0; i < concurrentTasks; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < operationsPerTask; j++)
                {
                    var key = $"concurrent_key_{random.Next(1, 20)}"; // Use limited key range to force contention
                    var value = $"value_{DateTime.UtcNow.Ticks}";

                    try
                    {
                        switch (random.Next(3))
                        {
                            case 0: // Put
                                cache.Put(key, value, TimeSpan.FromMinutes(1), out _);
                                break;
                            case 1: // Get
                                cache.TryGet(key, out _, out _);
                                break;
                            case 2: // Remove
                                cache.Remove(key, out _, out _);
                                break;
                        }
                        await Task.Delay(random.Next(1, 10)); // Random delay to increase contention chances
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in concurrent operation: {ex.Message}");
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("Concurrent operations completed.");
    }

    static async Task TestTtlAndExpiration(ILruCache<string, string> cache)
    {
        // Test very short TTLs
        Console.WriteLine("Testing short TTL items...");
        for (int i = 1; i <= 3; i++)
        {
            var key = $"short_ttl_key_{i}";
            cache.Put(key, $"value_{i}", TimeSpan.FromSeconds(2), out _);
        }

        // Wait for items to expire
        await Task.Delay(3000);

        // Verify items are expired
        for (int i = 1; i <= 3; i++)
        {
            var key = $"short_ttl_key_{i}";
            bool exists = cache.TryGet(key, out _, out var error);
            Console.WriteLine($"Short TTL item {key} exists after expiration: {exists}, Error: {error}");
        }

        // Test sliding expiration behavior
        Console.WriteLine("\nTesting sliding expiration...");
        cache.Put("sliding_key", "sliding_value", TimeSpan.FromSeconds(5), out _);

        for (int i = 0; i < 4; i++)
        {
            await Task.Delay(2000); // Wait 2 seconds
            bool exists = cache.TryGet("sliding_key", out var value, out var error);
            Console.WriteLine($"Sliding key access after {(i + 1) * 2}s: {exists}, Error: {error}");
        }
    }

    static async Task TestEdgeCases(ILruCache<string, string> cache)
    {
        Console.WriteLine("Testing edge cases...");

        // Test empty string key
        bool emptyKeySuccess = cache.Put("", "empty_key_value", null, out var emptyKeyError);
        Console.WriteLine($"Empty key put: {emptyKeySuccess}, Error: {emptyKeyError}");

        // Test null value
        bool nullValueSuccess = cache.Put("null_value_key", null, null, out var nullValueError);
        Console.WriteLine($"Null value put: {nullValueSuccess}, Error: {nullValueError}");

        // Test very large key
        var largeKey = new string('K', 10000);
        bool largeKeySuccess = cache.Put(largeKey, "large_key_value", null, out var largeKeyError);
        Console.WriteLine($"Large key put: {largeKeySuccess}, Error: {largeKeyError}");

        // Test rapid puts on same key
        Console.WriteLine("\nTesting rapid updates to same key...");
        for (int i = 0; i < 1000; i++)
        {
            cache.Put("rapid_key", $"value_{i}", null, out _);
        }

        // Test get after clear
        cache.Clear();
        bool getClearedSuccess = cache.TryGet("rapid_key", out _, out var getClearedError);
        Console.WriteLine($"Get after clear: {getClearedSuccess}, Error: {getClearedError}");
    }

    static void PrintStatistics(ICacheStats stats)
    {
        Console.WriteLine($"Total Requests: {stats.TotalRequests:N0}");
        Console.WriteLine($"Cache Hits: {stats.CacheHits:N0}");
        Console.WriteLine($"Cache Misses: {stats.CacheMisses:N0}");
        Console.WriteLine($"Hit Ratio: {stats.HitRatio:P2}");
        Console.WriteLine($"Eviction Count: {stats.EvictionCount:N0}");
        Console.WriteLine($"Expired Count: {stats.ExpiredCount:N0}");
        Console.WriteLine($"Current Item Count: {stats.CurrentItemCount:N0}");
        Console.WriteLine($"Total Memory: {stats.TotalMemoryBytes:N0} bytes");
    }

    static async Task TestConcurrentObjectOperations(ILruCache<string, TestObject> cache)
    {
        const int taskCount = 20;
        var tasks = new List<Task>();

        for (int i = 0; i < taskCount; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                var random = new Random();
                for (int j = 0; j < 50; j++)
                {
                    var obj = new TestObject
                    {
                        Id = taskId * 1000 + j,
                        Name = $"Task{taskId}_Object{j}",
                        Data = new byte[random.Next(512, 2048)]
                    };

                    try
                    {
                        cache.Put($"task{taskId}_obj{j}", obj, TimeSpan.FromMinutes(1), out _);
                        await Task.Delay(random.Next(1, 5));
                        cache.TryGet($"task{taskId}_obj{j}", out _, out _);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in concurrent object operation: {ex.Message}");
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}