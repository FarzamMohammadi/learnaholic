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
            TimeSpan.FromMilliseconds(50)
        );

        var operationBlocked = new ManualResetEventSlim();
        var blockOperation = new ManualResetEventSlim();

        // Setup storage to block
        _storage.Setup(s => s.TryPut(
            It.IsAny<string>(),
            It.IsAny<CacheItem<string>>(),
            out It.Ref<CacheAdditionError?>.IsAny))
            .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
            {
                operationBlocked.Set();
                blockOperation.Wait();
                err = null;
                return true;
            });

        // Start blocking operation
        var blockingTask = Task.Run(() => slowCache.Put("initial", "value", null, out _));
        Assert.True(operationBlocked.Wait(TimeSpan.FromSeconds(1)));

        // Verify all operations timeout while lock is held
        VerifyOperationTimeout(slowCache);

        // Cleanup
        blockOperation.Set();
        blockingTask.Wait();
    }

    [Fact]
    public void ConcurrentOperations_RespectLockingRules()
    {
        const int readerCount = 5;
        var readersStarted = new CountdownEvent(readerCount);
        var startOperations = new ManualResetEventSlim();
        var writeBlocked = new ManualResetEventSlim();
        var completeWrite = new ManualResetEventSlim();

        // Setup storage for concurrent operations
        SetupStorageGet(true, "value");
        _storage.Setup(s => s.TryPut(
            It.IsAny<string>(),
            It.IsAny<CacheItem<string>>(),
            out It.Ref<CacheAdditionError?>.IsAny))
            .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
            {
                writeBlocked.Set();
                completeWrite.Wait();
                err = null;
                return true;
            });

        // Start concurrent readers
        var readers = Enumerable.Range(0, readerCount).Select(_ => Task.Run(() =>
        {
            startOperations.Wait();
            var success = _cache.TryGet("key", out var _, out var error);
            Assert.True(success);
            Assert.Equal(CacheRetrievalError.None, error);
            readersStarted.Signal();
        })).ToList();

        // Start writer that will block
        var writer = Task.Run(() =>
        {
            startOperations.Wait();
            _cache.Put("key2", "value2", null, out _);
        });

        // Start operations and verify concurrent behavior
        startOperations.Set();
        
        // Verify readers complete while writer is blocked
        Assert.True(readersStarted.Wait(TimeSpan.FromSeconds(1)));
        Assert.True(writeBlocked.Wait(TimeSpan.FromSeconds(1)));

        // Complete write operation
        completeWrite.Set();
        writer.Wait();
    }

    private void VerifyOperationTimeout(LruCache<string, string> cache)
    {
        // Verify Put timeout
        var putSuccess = cache.Put("key", "value", null, out var putError);
        Assert.False(putSuccess);
        Assert.Equal(CacheAdditionError.ThreadLockTimeout, putError);

        // Verify Get timeout
        var getSuccess = cache.TryGet("key", out _, out var getError);
        Assert.False(getSuccess);
        Assert.Equal(CacheRetrievalError.ThreadLockTimeout, getError);

        // Verify Remove timeout
        var removeSuccess = cache.Remove("key", out _, out var removeError);
        Assert.False(removeSuccess);
        Assert.Equal(CacheRemovalError.ThreadLockTimeout, removeError);
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

/// <summary>
/// Tests for thread-safety and concurrency in LruCache
/// </summary>
[Collection("LruCache Tests")]
public class LruCacheConcurrencyTests
{
    private readonly Mock<ILruStorage<string, string>> _storage;
    private readonly Mock<ICacheStats> _stats;
    private readonly ILruPolicy _policy;
    private readonly LruCache<string, string> _cache;
    private const int DefaultLockTimeoutMs = 50;

    public LruCacheConcurrencyTests()
    {
        _storage = new Mock<ILruStorage<string, string>>();
        _stats = new Mock<ICacheStats>();
        _policy = new LruPolicy(TtlPolicy.None);
        _cache = new LruCache<string, string>(
            _storage.Object,
            _policy,
            _stats.Object,
            TimeSpan.FromMilliseconds(DefaultLockTimeoutMs)
        );
    }

    [Fact]
    public void Operations_HandleLockTimeout()
    {
        var operationBlocked = new ManualResetEventSlim();
        var releaseBlock = new ManualResetEventSlim();

        SetupBlockingStorage(operationBlocked, releaseBlock);

        using var blockingTask = Task.Run(() => _cache.Put("initial", "value", null, out _));
        Assert.True(operationBlocked.Wait(TimeSpan.FromSeconds(1)), "Initial operation failed to acquire lock");

        VerifyOperationTimeouts(_cache);

        releaseBlock.Set();
        blockingTask.Wait(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ConcurrentOperations_RespectLockingRules()
    {
        const int readerCount = 5;
        using var readersCompleted = new CountdownEvent(readerCount);
        using var startOperations = new ManualResetEventSlim();
        using var writeBlocked = new ManualResetEventSlim();
        using var completeWrite = new ManualResetEventSlim();

        SetupConcurrentOperationsStorage(writeBlocked, completeWrite);

        // Start concurrent readers
        var readers = StartConcurrentReaders(readerCount, startOperations, readersCompleted);
        var writer = StartBlockingWriter(startOperations);

        // Verify concurrent behavior
        startOperations.Set();
        Assert.True(readersCompleted.Wait(TimeSpan.FromSeconds(1)), "Readers failed to complete");
        Assert.True(writeBlocked.Wait(TimeSpan.FromSeconds(1)), "Write operation not blocked");

        // Cleanup
        completeWrite.Set();
        Task.WaitAll([writer, ..readers], TimeSpan.FromSeconds(1));
    }

    private void SetupBlockingStorage(ManualResetEventSlim operationBlocked, ManualResetEventSlim releaseBlock)
    {
        _storage.Setup(s => s.TryPut(
            It.IsAny<string>(),
            It.IsAny<CacheItem<string>>(),
            out It.Ref<CacheAdditionError?>.IsAny))
            .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
            {
                operationBlocked.Set();
                releaseBlock.Wait();
                err = null;
                return true;
            });
    }

    private void SetupConcurrentOperationsStorage(ManualResetEventSlim writeBlocked, ManualResetEventSlim completeWrite)
    {
        _storage.Setup(s => s.TryGet(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
            .Returns((string _, out CacheItem<string> item) =>
            {
                item = new CacheItem<string>("value");
                return true;
            });

        _storage.Setup(s => s.TryPut(
            It.IsAny<string>(),
            It.IsAny<CacheItem<string>>(),
            out It.Ref<CacheAdditionError?>.IsAny))
            .Returns((string _, CacheItem<string> _, out CacheAdditionError? err) =>
            {
                writeBlocked.Set();
                completeWrite.Wait();
                err = null;
                return true;
            });
    }

    private static Task[] StartConcurrentReaders(
        int count, 
        ManualResetEventSlim startSignal, 
        CountdownEvent completion)
    {
        return Enumerable.Range(0, count)
            .Select(_ => Task.Run(() =>
            {
                startSignal.Wait();
                completion.Signal();
            }))
            .ToArray();
    }

    private Task StartBlockingWriter(ManualResetEventSlim startSignal)
    {
        return Task.Run(() =>
        {
            startSignal.Wait();
            _cache.Put("key2", "value2", null, out _);
        });
    }

    private static void VerifyOperationTimeouts(ILruCache<string, string> cache)
    {
        // Verify Put timeout
        var putSuccess = cache.Put("key", "value", null, out var putError);
        Assert.False(putSuccess, "Put operation should timeout");
        Assert.Equal(CacheAdditionError.ThreadLockTimeout, putError);

        // Verify Get timeout
        var getSuccess = cache.TryGet("key", out _, out var getError);
        Assert.False(getSuccess, "Get operation should timeout");
        Assert.Equal(CacheRetrievalError.ThreadLockTimeout, getError);

        // Verify Remove timeout
        var removeSuccess = cache.Remove("key", out _, out var removeError);
        Assert.False(removeSuccess, "Remove operation should timeout");
        Assert.Equal(CacheRemovalError.ThreadLockTimeout, removeError);
    }
}