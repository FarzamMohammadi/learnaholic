using EnhancedLRUCache;
using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;
using Moq;
using Xunit;

namespace EnhancedLRUCacheTests;

public class LruCacheTests
{
    private readonly Mock<ILruStorage<string, string>> _storage = new();
    private readonly Mock<ICacheStats> _stats = new();
    private readonly ILruPolicy _policy;
    private readonly LruCache<string, string> _cache;
    private const int DefaultTimeoutMs = 100;

    public LruCacheTests()
    {
        _policy = new LruPolicy(TtlPolicy.Absolute, TimeSpan.FromMinutes(1));
        _cache = new LruCache<string, string>(
            _storage.Object,
            _policy,
            _stats.Object,
            TimeSpan.FromMilliseconds(DefaultTimeoutMs)
        );
    }

    [Fact]
    public void Constructor_ValidatesRequiredDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new LruCache<string, string>(null!, _policy, _stats.Object));
        Assert.Throws<ArgumentNullException>(() => new LruCache<string, string>(_storage.Object, null!, _stats.Object));
        Assert.Throws<ArgumentNullException>(() => new LruCache<string, string>(_storage.Object, _policy, null!));
    }

    [Fact]
    public void Put_AddsItemAndUpdatesStats()
    {
        SetupStoragePut(true);

        var success = _cache.Put("key", "value", TimeSpan.FromMinutes(1), out var error);

        Assert.True(success);
        Assert.Equal(CacheAdditionError.None, error);
        _stats.Verify(s => s.IncrementRequestCount(), Times.Once);
        _stats.Verify(s => s.UpdateItemCount(1), Times.Once);
    }

    [Fact]
    public void Put_HandlesStorageFailure()
    {
        SetupStoragePut(false, CacheAdditionError.MaxMemorySizeExceeded);

        var success = _cache.Put("key", "value", null, out var error);

        Assert.False(success);
        Assert.Equal(CacheAdditionError.MaxMemorySizeExceeded, error);
    }

    [Fact]
    public void TryGet_RetrievesItemAndUpdatesStats()
    {
        SetupStorageGet(true, "value");

        var success = _cache.TryGet("key", out var value, out var error);

        Assert.True(success);
        Assert.Equal("value", value);
        Assert.Equal(CacheRetrievalError.None, error);

        _stats.Verify(s => s.IncrementRequestCount(), Times.Once);
    }

    [Fact]
    public void TryGet_HandlesMissingItem()
    {
        SetupStorageGet(false);

        var success = _cache.TryGet("key", out var value, out var error);

        Assert.False(success);
        Assert.Equal(CacheRetrievalError.ItemNotFound, error);
        _stats.Verify(s => s.IncrementMissedRequestCount(), Times.Once);
    }

    [Fact]
    public void Remove_DeletesItemAndUpdatesStats()
    {
        var cacheItem = new CacheItem<string>("value");
        SetupStorageRemove(true, cacheItem);

        var success = _cache.Remove("key", out var value, out var error);

        Assert.True(success);
        Assert.Equal("value", value);
        Assert.Equal(CacheRemovalError.None, error);
        _stats.Verify(s => s.UpdateItemCount(-1), Times.Once);
    }

    [Fact]
    public void Clear_ResetsStorageAndStats()
    {
        var success = _cache.Clear();

        Assert.True(success);
        _storage.Verify(s => s.Clear(), Times.Once);
        _stats.Verify(s => s.ClearMetrics(), Times.Once);
    }

    [Fact]
    public void GetExpiredKeys_ReturnsExpiredItems()
    {
        var expiredKeys = new[] { "key1", "key2" };
        _storage.Setup(s => s.GetExpiredKeys()).Returns(expiredKeys);

        var result = _cache.GetExpiredKeys(out var error);

        Assert.Equal(expiredKeys, result);
        Assert.Equal(CacheRetrievalError.None, error);
    }

    // [Theory]
    // [InlineData(true)]  // ItemEvicted
    // [InlineData(false)] // ItemExpired
    // public void Events_PropagateCorrectly(bool isEvicted)
    // {
    //     var item = new CacheItemEventArgs<string, string>("key", "value", DateTime.UtcNow);
    //     var eventRaised = false;
    //
    //     if (isEvicted)
    //     {
    //         _cache.ItemEvicted += (_, _) => eventRaised = true;
    //         _storage.Raise(s => s.ItemEvicted += null, null, item);
    //     }
    //     else
    //     {
    //         _cache.ItemExpired += (_, _) => eventRaised = true;
    //         _custo.Raise(s => s.ItemExpired += null, null, item);
    //     }
    //
    //     Assert.True(eventRaised);
    // }

    [Fact]
    public void Dispose_PreventsFurtherOperations()
    {
        _cache.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
            _cache.Put("key", "value", null, out _)
        );
        Assert.Throws<ObjectDisposedException>(() =>
            _cache.TryGet("key", out _, out _)
        );
    }

    [Fact]
    public void Operations_HandleLockTimeout()
    {
        var slowCache = new LruCache<string, string>(
            _storage.Object,
            _policy,
            _stats.Object,
            TimeSpan.FromMilliseconds(1) // Very short timeout
        );

        // Hold write lock to force timeout
        _storage.Setup(s => s.TryPut(It.IsAny<string>(), It.IsAny<CacheItem<string>>(), out It.Ref<CacheAdditionError?>.IsAny))
                .Callback(() => Thread.Sleep(10));

        var putSuccess = slowCache.Put("key", "value", null, out var putError);
        Assert.False(putSuccess);
        Assert.Equal(CacheAdditionError.ThreadLockTimeout, putError);
    }

    [Theory]
    [InlineData(TtlPolicy.Absolute)]
    [InlineData(TtlPolicy.Sliding)]
    public void Put_CreatesCorrectExpirationBasedOnPolicy(TtlPolicy policyType)
    {
        var policy = new LruPolicy(policyType, TimeSpan.FromMinutes(5));
        var cache = new LruCache<string, string>(_storage.Object, policy, _stats.Object);

        cache.Put("key", "value", TimeSpan.FromMinutes(1), out _);

        _storage.Verify(s => s.TryPut(
                It.IsAny<string>(),
                It.Is<CacheItem<string>>(item =>
                    policyType == TtlPolicy.Absolute
                        ? item.AbsoluteExpiration.HasValue && !item.SlidingExpiration.HasValue
                        : item.SlidingExpiration.HasValue && !item.AbsoluteExpiration.HasValue
                ),
                out It.Ref<CacheAdditionError?>.IsAny
            )
        );
    }

    [Fact]
    public void Put_UpdatesMemoryStats()
    {
        var item = new CacheItem<string>("value");
        SetupStoragePut(true);

        _cache.Put("key", "value", null, out _);

        _stats.Verify(s => s.UpdateMemory(item.Size), Times.Once);
    }

    private void SetupStoragePut(bool success, CacheAdditionError? error = null)
    {
        _storage.Setup(s => s.TryPut(
                        It.IsAny<string>(),
                        It.IsAny<CacheItem<string>>(),
                        out It.Ref<CacheAdditionError?>.IsAny
                    )
                )
                .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
                    {
                        err = error;
                        return success;
                    }
                );
    }

    private void SetupStorageGet(bool success, string? value = null)
    {
        _storage.Setup(s => s.TryGet(
                        It.IsAny<string>(),
                        out It.Ref<CacheItem<string>>.IsAny
                    )
                )
                .Returns((string _, out CacheItem<string> item) =>
                    {
                        item = success ? new CacheItem<string>(value) : default!;
                        return success;
                    }
                );
    }

    private void SetupStorageRemove(bool success, CacheItem<string> item)
    {
        _storage.Setup(s => s.TryGet(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
                .Returns((string _, out CacheItem<string> outItem) =>
                    {
                        outItem = success ? item : default!;
                        return success;
                    }
                );
        _storage.Setup(s => s.Remove(It.IsAny<string>()));
    }

    // [Fact]
    // public void Dispose_CleanupsCustodianAndLock()
    // {
    //     var custodian = new Mock<ICacheCustodian>();
    //     // Create cache with mocked custodian
    //     var cache = new LruCache<string, string>(_storage.Object, _policy, _stats.Object);
    //
    //     cache.Dispose();
    //
    //     // Verify custodian is disposed
    //     custodian.Verify(c => c.Dispose(), Times.Once);
    // }

    [Theory]
    [InlineData(null, null, null)] // Default values
    [InlineData(1000, 5000, 1000)] // Custom values
    public void Constructor_InitializesWithOptionalParameters(
        int? lockTimeoutMs,
        int? cleanupIntervalMs,
        int? retryIntervalMs)
    {
        var cache = new LruCache<string, string>(
            _storage.Object,
            _policy,
            _stats.Object,
            lockTimeoutMs.HasValue ? TimeSpan.FromMilliseconds(lockTimeoutMs.Value) : null,
            cleanupIntervalMs.HasValue ? TimeSpan.FromMilliseconds(cleanupIntervalMs.Value) : null,
            retryIntervalMs.HasValue ? TimeSpan.FromMilliseconds(retryIntervalMs.Value) : null
        );

        // Verify initialization through behavior
        cache.Put("key", "value", null, out _);
        _storage.Verify(s => s.TryPut(It.IsAny<string>(), It.IsAny<CacheItem<string>>(), out It.Ref<CacheAdditionError?>.IsAny));
    }

    // [Fact]
    // public async Task TryGet_SchedulesExpiredItemRemoval()
    // {
    //     var expiredItem = new CacheItem<string>("value") { AbsoluteExpiration = DateTime.UtcNow.AddMinutes(-1) };
    //     SetupStorageGet(true, expiredItem);
    //
    //     _cache.TryGet("key", out _, out _);
    //
    //     // Allow time for async removal
    //     await Task.Delay(100);
    //
    //     _storage.Verify(s => s.Remove("key"), Times.Once);
    // }

    // [Fact]
    // public void Dispose_CleansUpResourcesAndPreventsOperations()
    // {
    //     var custodian = new Mock<ICacheCustodian>();
    //     var cache = new LruCache<string, string>(
    //         _storage.Object,
    //         _policy,
    //         _stats.Object
    //     );
    //
    //     cache.Dispose();
    //     cache.Dispose(); // Should handle multiple calls
    //
    //     // Verify operations throw
    //     Assert.Throws<ObjectDisposedException>(() => cache.Put("key", "value", null, out _));
    //     Assert.Throws<ObjectDisposedException>(() => cache.TryGet("key", out _, out _));
    //     Assert.Throws<ObjectDisposedException>(() => cache.Remove("key", out _, out _));
    //     Assert.Throws<ObjectDisposedException>(() => cache.GetExpiredKeys(out _));
    //     Assert.Throws<ObjectDisposedException>(() => cache.Clear());
    // }
}