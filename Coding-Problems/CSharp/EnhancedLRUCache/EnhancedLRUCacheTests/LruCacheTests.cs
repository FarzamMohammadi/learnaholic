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

    [Theory]
    [InlineData(true)]  // ItemEvicted
    [InlineData(false)] // ItemExpired
    public void Events_PropagateCorrectly(bool isEvicted)
    {
        var eventRaised = false;
        var testKey = "key";
        var testValue = "value";

        if (isEvicted)
        {
            var eventArgs = new CacheItemEventArgs<string, string>(testKey, testValue, DateTime.UtcNow);
            _cache.ItemEvicted += (_, _) => eventRaised = true;
            _storage.Raise(s => s.ItemEvicted += null, this, eventArgs);
        }
        else
        {
            _cache.ItemExpired += (_, _) => eventRaised = true;

            // Create a valid cache item that expires in the future
            var expiredItem = new CacheItem<string>(testValue, DateTime.UtcNow.AddMilliseconds(50));

            _storage.Setup(s => s.TryGet(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
                    .Returns((string _, out CacheItem<string> item) =>
                    {
                        item = expiredItem;
                        return true;
                    });

            // Wait for it to expire
            Thread.Sleep(51);

            _cache.TryGet(testKey, out _, out _);
        }

        Assert.True(eventRaised);
    }

    [Fact]
    public void Dispose_PreventsFurtherOperations()
    {
        _cache.Dispose();
        _cache.Dispose(); // Should handle multiple calls

        // Verify all operations throw ObjectDisposedException
        Assert.Throws<ObjectDisposedException>(() => _cache.Put("key", "value", null, out _));
        Assert.Throws<ObjectDisposedException>(() => _cache.TryGet("key", out _, out _));
        Assert.Throws<ObjectDisposedException>(() => _cache.Remove("key", out _, out _));
        Assert.Throws<ObjectDisposedException>(() => _cache.GetExpiredKeys(out _));
        Assert.Throws<ObjectDisposedException>(() => _cache.Clear());
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

    [Fact]
    public void Operations_HandleLockTimeout()
    {
        var slowCache = new LruCache<string, string>(
            _storage.Object,
            _policy,
            _stats.Object,
            TimeSpan.FromMilliseconds(50) // Short timeout
        );

        var lockHeld = new ManualResetEventSlim();
        var blockOperation = new ManualResetEventSlim();

        // Setup storage to block on the first operation
        _storage.Setup(s => s.TryPut(
                    It.IsAny<string>(),
                    It.IsAny<CacheItem<string>>(),
                    out It.Ref<CacheAdditionError?>.IsAny))
                .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
                {
                    lockHeld.Set();
                    blockOperation.Wait(); // Block the operation
                    err = null;
                    return true;
                });

        // Start a task that holds the write lock
        var lockingTask = Task.Run(() => { slowCache.Put("initial", "value", null, out _); });

        // Wait until the lock is held
        Assert.True(lockHeld.Wait(TimeSpan.FromSeconds(1)));

        // Try operations while lock is held
        var putSuccess = slowCache.Put("key", "value", null, out var putError);
        Assert.False(putSuccess);
        Assert.Equal(CacheAdditionError.ThreadLockTimeout, putError);

        var getSuccess = slowCache.TryGet("key", out _, out var getError);
        Assert.False(getSuccess);
        Assert.Equal(CacheRetrievalError.ThreadLockTimeout, getError);

        var removeSuccess = slowCache.Remove("key", out _, out var removeError);
        Assert.False(removeSuccess);
        Assert.Equal(CacheRemovalError.ThreadLockTimeout, removeError);

        // Cleanup
        blockOperation.Set();
        lockingTask.Wait();
    }

    [Fact]
    public void ConcurrentReads_AreAllowed()
    {
        const int readerCount = 10;
        var readComplete = new CountdownEvent(readerCount);
        var startReading = new ManualResetEventSlim();

        // Setup initial value with proper mock
        SetupStorageGet(true, "value");
        _cache.Put("key", "value", null, out _);

        // Start multiple concurrent readers
        var tasks = Enumerable.Range(0, readerCount).Select(_ => Task.Run(() =>
        {
            startReading.Wait();
            _cache.TryGet("key", out var _, out var _);
            readComplete.Signal();
        })).ToArray();

        startReading.Set();

        // All reads should complete without timeout
        Assert.True(readComplete.Wait(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void WriteLock_PreventsOtherWrites()
    {
        var writerStarted = new ManualResetEventSlim();
        var canFinishWrite = new ManualResetEventSlim();
        var secondWriteAttempted = new ManualResetEventSlim();

        // Setup storage to delay on first write
        _storage.Setup(s => s.TryPut(
                    It.IsAny<string>(),
                    It.IsAny<CacheItem<string>>(),
                    out It.Ref<CacheAdditionError?>.IsAny))
                .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
                {
                    writerStarted.Set();
                    canFinishWrite.Wait(); // Hold the storage operation
                    err = null;
                    return true;
                });

        // First writer
        var firstWriter = Task.Run(() => { _cache.Put("key1", "value1", null, out _); });

        // Wait for first writer to start
        Assert.True(writerStarted.Wait(TimeSpan.FromSeconds(1)));

        // Second writer should timeout
        var secondWriter = Task.Run(() =>
        {
            var success = _cache.Put("key2", "value2", null, out var error);
            Assert.False(success);
            Assert.Equal(CacheAdditionError.ThreadLockTimeout, error);
            secondWriteAttempted.Set();
        });

        // Wait for second write attempt
        Assert.True(secondWriteAttempted.Wait(TimeSpan.FromSeconds(1)));

        // Cleanup
        canFinishWrite.Set();
        Task.WaitAll(firstWriter, secondWriter);
    }

    [Fact]
    public void TryGet_SchedulesExpiredItemRemoval()
    {
        // Create a valid cache item that expires in the future
        var expiredItem = new CacheItem<string>("value", DateTime.UtcNow.AddMilliseconds(50));

        _storage.Setup(s => s.TryGet(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
                .Returns((string _, out CacheItem<string> item) =>
                {
                    item = expiredItem;
                    return true;
                });

        // Wait for it to expire
        Thread.Sleep(51);

        _cache.TryGet("key", out _, out _);

        // Allow time for async removal
        Thread.Sleep(100);

        _storage.Verify(s => s.Remove("key"), Times.Once);
    }
}