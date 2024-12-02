// Custom assertion implementation for testing purposes
// Simulates basic unit testing functionality without requiring test framework dependencies
// Common approach in coding interviews where external testing frameworks aren't available

using System.ComponentModel.DataAnnotations;
using LRUCache;

TestBasicPutAndGet();
TestCapacityEviction();
TestUpdateExistingKey();
TestLRUEvictionOrder();
TestErrorCases();

Console.WriteLine("All tests passed!");

static void TestBasicPutAndGet()
{
    var cache = new Cache(2);
    cache.Put("key1", 1);
    Assert(cache.Get("key1") == 1);
}

static void TestCapacityEviction()
{
    var cache = new Cache(2);

    // Fill cache
    cache.Put("key1", 1);
    cache.Put("key2", 2);
    Assert(cache.Get("key1") == 1);
    Assert(cache.Get("key2") == 2);

    // Should evict key1 (least recently used)
    cache.Put("key3", 3);

    try
    {
        cache.Get("key1");
        Assert(false); // Should not reach here
    }
    catch (KeyNotFoundException)
    {
        Assert(true);
    }

    Assert(cache.Get("key2") == 2);
    Assert(cache.Get("key3") == 3);
}

static void TestUpdateExistingKey()
{
    var cache = new Cache(2);

    cache.Put("key1", 1);
    cache.Put("key1", 2); // Update value
    Assert(cache.Get("key1") == 2);

    cache.Put("key2", 2);
    cache.Put("key1", 3); // Update should not cause eviction
    Assert(cache.Get("key2") == 2);
}

static void TestLRUEvictionOrder()
{
    var cache = new Cache(3);

    cache.Put("key1", 1);
    cache.Put("key2", 2);
    cache.Put("key3", 3);

    // Access key1, making key2 the least recently used
    cache.Get("key1");
    cache.Get("key3");

    // This should evict key2
    cache.Put("key4", 4);

    try
    {
        cache.Get("key2");
        Assert(false); // Should not reach here
    }
    catch (KeyNotFoundException)
    {
        Assert(true);
    }

    Assert(cache.Get("key1") == 1);
    Assert(cache.Get("key3") == 3);
    Assert(cache.Get("key4") == 4);
}

static void TestErrorCases()
{
    // Test negative capacity
    try
    {
        _ = new Cache(-1);
        Assert(false); // Should not reach here
    }
    catch (ArgumentException)
    {
        Assert(true);
    }

    var validCache = new Cache(1);

    // Test null key
    try
    {
        validCache.Put(null!, 1);
        Assert(false); // Should not reach here
    }
    catch (ArgumentException)
    {
        Assert(true);
    }

    // Test empty key
    try
    {
        validCache.Put("", 1);
        Assert(false); // Should not reach here
    }
    catch (ArgumentException)
    {
        Assert(true);
    }

    // Test getting non-existent key
    try
    {
        validCache.Get("nonexistent");
        Assert(false); // Should not reach here
    }
    catch (KeyNotFoundException)
    {
        Assert(true);
    }
}

static void Assert(bool condition)
{
    if (condition is not true) throw new ValidationException($"Expected {true} but got {false}");
}