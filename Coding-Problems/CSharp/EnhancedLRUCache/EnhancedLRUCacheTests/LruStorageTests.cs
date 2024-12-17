using EnhancedLRUCache;
using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;
using Moq;
using Xunit;

namespace EnhancedLRUCacheTests;

public class LruStorageTests
{
    private readonly Mock<ICacheStats> _stats = new();
    private readonly LruStorage<string, string> _storage;
    private const int MaxItems = 3;
    private const int MaxMemory = 200;

    public LruStorageTests()
    {
        _storage = new LruStorage<string, string>(MaxItems, MaxMemory, _stats.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ValidatesMaxItemCount(int maxItems)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LruStorage<string, string>(maxItems, MaxMemory, _stats.Object)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ValidatesMaxMemorySize(int maxMemory)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LruStorage<string, string>(MaxItems, maxMemory, _stats.Object)
        );
    }

    [Fact]
    public void TryPut_AddsItemAndUpdatesMemory()
    {
        var cacheItem = new CacheItem<string>("value");
        var success = _storage.TryPut("key", cacheItem, out var error);

        Assert.True(success);
        Assert.Equal(CacheAdditionError.None, error);
        Assert.Equal(1, _storage.Count);
        Assert.Equal(cacheItem.Size, _storage.CurrentMemorySize);
    }

    [Fact]
    public void TryPut_UpdatesExistingItem()
    {
        var initialItem = new CacheItem<string>("initial");
        var updatedItem = new CacheItem<string>("updated");

        _storage.TryPut("key", initialItem, out _);
        _storage.TryPut("key", updatedItem, out var error);

        Assert.Equal(CacheAdditionError.None, error);
        Assert.Equal(1, _storage.Count);
        Assert.Equal(updatedItem.Size, _storage.CurrentMemorySize);
    }

    [Fact]
    public void TryPut_EvictsLeastRecentlyUsedWhenFull()
    {
        var evictionCount = 0;
        _storage.ItemEvicted += (_, _) => evictionCount++;

        for (int i = 0; i < MaxItems + 1; i++)
        {
            _storage.TryPut($"key{i}", new CacheItem<string>($"value{i}"), out _);
        }

        Assert.Equal(MaxItems, _storage.Count);
        Assert.Equal(1, evictionCount);
        Assert.False(_storage.ContainsKey("key0"));
    }

    [Fact]
    public void TryGet_UpdatesAccessOrder()
    {
        _storage.TryPut("key1", new CacheItem<string>("value1"), out _);
        _storage.TryPut("key2", new CacheItem<string>("value2"), out _);
        _storage.TryPut("key3", new CacheItem<string>("value3"), out _);

        _storage.TryGet("key1", out _);                                  // Move key1 to front
        _storage.TryPut("key4", new CacheItem<string>("value4"), out _); // Should evict key2

        Assert.True(_storage.ContainsKey("key1"));
        Assert.False(_storage.ContainsKey("key2"));
    }

    [Fact]
    public async Task ExpiredItem_ShouldBeInExpiredKeysCollection()
    {
        const string expiredKey = "expired";
        const string validKey = "valid";

        var expiredItem = new CacheItem<string>(expiredKey, DateTime.UtcNow.AddSeconds(1));
        var validItem = new CacheItem<string>(validKey, DateTime.UtcNow.AddMinutes(1));

        _storage.TryPut(expiredKey, expiredItem, out _);
        _storage.TryPut(validKey, validItem, out _);

        await Task.Delay(TimeSpan.FromSeconds(1));

        var hasExpiredItem = _storage.ContainsKey(expiredKey);
        var hasValidItem = _storage.ContainsKey(validKey);
        var expiredKeys = _storage.GetExpiredKeys();

        Assert.True(hasExpiredItem);
        Assert.True(hasValidItem);
        Assert.Contains(expiredKey, expiredKeys);
    }

    [Fact]
    public void Clear_RemovesAllItemsAndResetsMemory()
    {
        _storage.TryPut("key1", new CacheItem<string>("value1"), out _);
        _storage.TryPut("key2", new CacheItem<string>("value2"), out _);

        _storage.Clear();

        Assert.Equal(0, _storage.Count);
        Assert.Equal(0, _storage.CurrentMemorySize);
        Assert.False(_storage.ContainsKey("key1"));
    }

    [Fact]
    public void TryPut_RejectsItemsExceedingMaxMemory()
    {
        var largeItem = new CacheItem<string>(new string('x', MaxMemory + 1));

        var success = _storage.TryPut("key", largeItem, out var error);

        Assert.False(success);
        Assert.Equal(CacheAdditionError.MaxMemorySizeExceeded, error);
        Assert.Equal(0, _storage.Count);
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

        _storage.TryPut("key1", largeItem1, out _);
        _storage.TryPut("key2", largeItem2, out _);
        _storage.TryPut("key3", largeItem3, out _);

        // This should cause eviction of key1
        _storage.TryPut("key4", largeItem4, out var error);

        Assert.Equal(CacheAdditionError.None, error);
        Assert.False(_storage.ContainsKey("key1"));
        Assert.True(_storage.ContainsKey("key2"));
        Assert.True(_storage.ContainsKey("key3"));
        Assert.True(_storage.ContainsKey("key4"));
    }

    [Fact]
    public void CurrentMemorySize_TracksAccurately()
    {
        var item1 = new CacheItem<string>("small");
        var item2 = new CacheItem<string>(new string('x', 50));

        _storage.TryPut("key1", item1, out _);
        var sizeAfterFirst = _storage.CurrentMemorySize;

        _storage.TryPut("key2", item2, out _);
        var sizeAfterSecond = _storage.CurrentMemorySize;

        _storage.Remove("key1");
        var sizeAfterRemove = _storage.CurrentMemorySize;

        Assert.Equal(item1.Size, sizeAfterFirst);
        Assert.Equal(item1.Size + item2.Size, sizeAfterSecond);
        Assert.Equal(item2.Size, sizeAfterRemove);
    }

    [Fact]
    public void Operations_ValidateNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => _storage.TryGet(null!, out _));
        Assert.Throws<ArgumentNullException>(() => _storage.TryPut(null!, new CacheItem<string>("value"), out _));
        Assert.Throws<ArgumentNullException>(() => _storage.TryPut("key", null!, out _));
        Assert.Throws<ArgumentNullException>(() => _storage.Remove(null!));
    }

    [Fact]
    public void ItemEvicted_NotRaisedForManualRemoval()
    {
        var evictionCount = 0;
        _storage.ItemEvicted += (_, _) => evictionCount++;

        _storage.TryPut("key", new CacheItem<string>("value"), out _);
        _storage.Remove("key");

        Assert.Equal(0, evictionCount);
    }

    [Fact]
    public void IsFull_ReflectsMaxItemCount()
    {
        Assert.False(_storage.IsFull);

        for (int i = 0; i <= MaxItems; i++)
        {
            _storage.TryPut($"key{i}", new CacheItem<string>($"value{i}"), out _);
        }

        Assert.True(_storage.IsFull);
    }
}