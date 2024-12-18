using EnhancedLRUCache.Caching.Core;
using EnhancedLRUCache.Caching.Errors;
using EnhancedLRUCache.Caching.Payload;
using Xunit;

namespace EnhancedLRUCacheTests;

public class CacheStorageTests
{
    private CacheStorage<string, string> _cacheStorage = new(MaxItems, MaxMemory);

    private const int MaxItems = 3;
    private const int MaxMemory = 200;

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ValidatesMaxItemCount(int maxItems)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CacheStorage<string, string>(maxItems, MaxMemory)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ValidatesMaxMemorySize(int maxMemory)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CacheStorage<string, string>(MaxItems, maxMemory)
        );
    }

    [Fact]
    public void TryPut_AddsItemAndUpdatesMemory()
    {
        var cacheItem = new CacheItem<string>("value");
        var success = _cacheStorage.TryPut("key", cacheItem, out var error);

        Assert.True(success);
        Assert.Equal(CacheAdditionError.None, error);
        Assert.Equal(1, _cacheStorage.Count);
        Assert.Equal(cacheItem.Size, _cacheStorage.CurrentMemorySize);
    }

    [Fact]
    public void TryPut_UpdatesExistingItem()
    {
        var initialItem = new CacheItem<string>("initial");
        var updatedItem = new CacheItem<string>("updated");

        _cacheStorage.TryPut("key", initialItem, out _);
        _cacheStorage.TryPut("key", updatedItem, out var error);

        Assert.Equal(CacheAdditionError.None, error);
        Assert.Equal(1, _cacheStorage.Count);
        Assert.Equal(updatedItem.Size, _cacheStorage.CurrentMemorySize);
    }

    [Fact]
    public void TryPut_EvictsLeastRecentlyUsedWhenFull()
    {
        var evictionCount = 0;
        _cacheStorage.ItemEvicted += (_, _) => evictionCount++;

        for (var i = 0; i < MaxItems + 1; i++)
        {
            _cacheStorage.TryPut($"key{i}", new CacheItem<string>($"value{i}"), out _);
        }

        Assert.Equal(MaxItems, _cacheStorage.Count);
        Assert.Equal(1, evictionCount);
        Assert.False(_cacheStorage.ContainsKey("key0"));
    }

    [Fact]
    public void TryGet_UpdatesAccessOrder()
    {
        _cacheStorage.TryPut("key1", new CacheItem<string>("value1"), out _);
        _cacheStorage.TryPut("key2", new CacheItem<string>("value2"), out _);
        _cacheStorage.TryPut("key3", new CacheItem<string>("value3"), out _);

        _cacheStorage.TryGet("key1", out _);                                  // Move key1 to front
        _cacheStorage.TryPut("key4", new CacheItem<string>("value4"), out _); // Should evict key2

        Assert.True(_cacheStorage.ContainsKey("key1"));
        Assert.False(_cacheStorage.ContainsKey("key2"));
    }

    [Fact]
    public async Task ExpiredItem_ShouldBeInExpiredKeysCollection()
    {
        const string expiredKey = "expired";
        const string validKey = "valid";

        var expiredItem = new CacheItem<string>(expiredKey, DateTime.UtcNow.AddSeconds(1));
        var validItem = new CacheItem<string>(validKey, DateTime.UtcNow.AddMinutes(1));

        _cacheStorage.TryPut(expiredKey, expiredItem, out _);
        _cacheStorage.TryPut(validKey, validItem, out _);

        await Task.Delay(TimeSpan.FromSeconds(1));

        var hasExpiredItem = _cacheStorage.ContainsKey(expiredKey);
        var hasValidItem = _cacheStorage.ContainsKey(validKey);
        var expiredKeys = _cacheStorage.GetExpiredKeys();

        Assert.True(hasExpiredItem);
        Assert.True(hasValidItem);
        Assert.Contains(expiredKey, expiredKeys);
    }

    [Fact]
    public void Clear_RemovesAllItemsAndResetsMemory()
    {
        _cacheStorage.TryPut("key1", new CacheItem<string>("value1"), out _);
        _cacheStorage.TryPut("key2", new CacheItem<string>("value2"), out _);

        _cacheStorage.Clear();

        Assert.Equal(0, _cacheStorage.Count);
        Assert.Equal(0, _cacheStorage.CurrentMemorySize);
        Assert.False(_cacheStorage.ContainsKey("key1"));
    }

    [Fact]
    public void TryPut_RejectsItemsExceedingMaxMemory()
    {
        var largeItem = new CacheItem<string>(new string('x', MaxMemory + 1));

        var success = _cacheStorage.TryPut("key", largeItem, out var error);

        Assert.False(success);
        Assert.Equal(CacheAdditionError.MaxMemorySizeExceeded, error);
        Assert.Equal(0, _cacheStorage.Count);
    }

    [Fact]
    public void TryPut_EvictsItemsUntilMemoryAvailable()
    {
        // For a string with length 20:
        // Size = 16 + 4 + (2 * 20) = 60 bytes
        const int largeStringLength = 20;

        // For a string with length 5:
        // Size = 16 + 4 + (2 * 5) = 30 bytes
        const int smallStringLength = 5;

        var largeItem1 = new CacheItem<string>(new string('x', largeStringLength));
        var largeItem2 = new CacheItem<string>(new string('x', largeStringLength));
        var largeItem3 = new CacheItem<string>(new string('x', largeStringLength));
        var largeItem4 = new CacheItem<string>(new string('x', smallStringLength));

        _cacheStorage.TryPut("key1", largeItem1, out _);
        _cacheStorage.TryPut("key2", largeItem2, out _);
        _cacheStorage.TryPut("key3", largeItem3, out _);

        // This should cause eviction of key1
        _cacheStorage.TryPut("key4", largeItem4, out var error);

        Assert.Equal(CacheAdditionError.None, error);
        Assert.False(_cacheStorage.ContainsKey("key1"));
        Assert.True(_cacheStorage.ContainsKey("key2"));
        Assert.True(_cacheStorage.ContainsKey("key3"));
        Assert.True(_cacheStorage.ContainsKey("key4"));
    }

    [Fact]
    public void CurrentMemorySize_TracksAccurately()
    {
        var item1 = new CacheItem<string>("small");
        var item2 = new CacheItem<string>(new string('x', 50));

        _cacheStorage.TryPut("key1", item1, out _);
        var sizeAfterFirst = _cacheStorage.CurrentMemorySize;

        _cacheStorage.TryPut("key2", item2, out _);
        var sizeAfterSecond = _cacheStorage.CurrentMemorySize;

        _cacheStorage.Remove("key1");
        var sizeAfterRemove = _cacheStorage.CurrentMemorySize;

        Assert.Equal(item1.Size, sizeAfterFirst);
        Assert.Equal(item1.Size + item2.Size, sizeAfterSecond);
        Assert.Equal(item2.Size, sizeAfterRemove);
    }

    [Fact]
    public void Operations_ValidateNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => _cacheStorage.TryGet(null!, out _));
        Assert.Throws<ArgumentNullException>(() => _cacheStorage.TryPut(null!, new CacheItem<string>("value"), out _));
        Assert.Throws<ArgumentNullException>(() => _cacheStorage.TryPut("key", null!, out _));
        Assert.Throws<ArgumentNullException>(() => _cacheStorage.Remove(null!));
    }

    [Fact]
    public void ItemEvicted_NotRaisedForManualRemoval()
    {
        var evictionCount = 0;
        _cacheStorage.ItemEvicted += (_, _) => evictionCount++;

        _cacheStorage.TryPut("key", new CacheItem<string>("value"), out _);
        _cacheStorage.Remove("key");

        Assert.Equal(0, evictionCount);
    }

    [Fact]
    public void IsFull_ReflectsMaxItemCount()
    {
        Assert.False(_cacheStorage.CountIsFull);

        for (var i = 0; i <= MaxItems; i++)
        {
            _cacheStorage.TryPut($"key{i}", new CacheItem<string>($"value{i}"), out _);
        }

        Assert.True(_cacheStorage.CountIsFull);
    }

    [Fact]
    public void IsFull_ReflectsMaxMemoryCount()
    {
        _cacheStorage = new CacheStorage<string, string>(MaxItems, 20);

        Assert.False(_cacheStorage.MemoryIsFull);

        for (var i = MaxMemory; i > 0; i -= 20)
        {
            _cacheStorage.TryPut($"key{i}", new CacheItem<string>(""), out _);
        }

        Assert.True(_cacheStorage.MemoryIsFull);
    }
}