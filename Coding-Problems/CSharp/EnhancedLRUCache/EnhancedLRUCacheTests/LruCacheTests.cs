using System.Collections.Concurrent;
using EnhancedLRUCache;
using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;
using Moq;
using Xunit;

namespace EnhancedLRUCacheTests;

/// <summary>
/// Tests for basic functionality, initialization, and disposal
/// </summary>
public class LruCacheTests
{
    private readonly Mock<ILruStorage<string, string>> _storage = new();
    private readonly Mock<ICacheStats> _stats = new();
    private readonly ILruPolicy _policy;
    private LruCache<string, string> _cache;
    private const int DefaultTimeoutMs = 100;
    private const int DefaultLockTimeoutMs = 100;
    private const int DefaultOperationTimeoutMs = 5000;

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

        var success = _cache.TryGet("key", out _, out var error);

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
    [InlineData(true)]  // ItemEvicted from Storage
    [InlineData(false)] // ItemExpired from Custodian
    public async Task Events_PropagateCorrectly(bool isEvicted)
    {
        var eventRaised = new TaskCompletionSource<bool>();
        const string testKey = "key";
        const string testValue = "value";

        if (isEvicted)
        {
            _cache.ItemEvicted += (_, _) => eventRaised.SetResult(true);

            var eventArgs = new CacheItemEventArgs<string, string>(testKey, testValue, DateTime.UtcNow);

            _storage.Raise(s => s.ItemEvicted += null, _storage.Object, eventArgs);
        }
        else
        {
            var cleanupInterval = TimeSpan.FromMilliseconds(10);

            _cache = new LruCache<string, string>(
                _storage.Object,
                _policy,
                _stats.Object,
                TimeSpan.FromMilliseconds(DefaultTimeoutMs),
                cleanupInterval
            );

            _cache.ItemExpired += (_, _) => eventRaised.SetResult(true);

            // Setup expired item in storage
            var expiredItem = new CacheItem<string>(testValue, DateTime.UtcNow.AddMilliseconds(1));

            _storage.Setup(s => s.GetExpiredKeys())
                    .Returns([testKey])
                    .Callback(() =>
                    {
                        // After first call, return empty to simulate item being removed
                        _storage.Setup(s => s.GetExpiredKeys()).Returns(Array.Empty<string>());
                    });

            _storage.Setup(s => s.TryGetWithoutRefresh(testKey, out It.Ref<CacheItem<string>>.IsAny))
                    .Returns((string _, out CacheItem<string> item) =>
                    {
                        item = expiredItem;
                        return true;
                    });

            _storage.Setup(s => s.TryGet(testKey, out It.Ref<CacheItem<string>>.IsAny))
                    .Returns((string _, out CacheItem<string> item) =>
                    {
                        item = expiredItem;
                        return true;
                    });

            // Setup Remove to return success
            _storage.Setup(s => s.Remove(testKey));

            // No need to create a new custodian - the cache already has one with the short interval
            await Task.Delay(TimeSpan.FromMilliseconds(50)); // Give custodian time to run
        }

        var raised = await eventRaised.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(raised, "Event was not raised within expected timeframe");
    }

    [Fact]
    public async Task Storage_ExpiredItem_RaisesEvent()
    {
        var cache = new LruCache<string, string>(
            _storage.Object,
            _policy,
            _stats.Object,
            TimeSpan.FromMilliseconds(DefaultTimeoutMs)
        );

        var eventRaised = new TaskCompletionSource<bool>();
        const string testKey = "key";
        const string testValue = "value";

        cache.ItemExpired += (_, _) => eventRaised.SetResult(true);

        var expiredItem = new CacheItem<string>(testValue, DateTime.UtcNow.AddMilliseconds(1));

        _storage.Setup(s => s.TryGet(testKey, out It.Ref<CacheItem<string>>.IsAny))
                .Returns((string _, out CacheItem<string> item) =>
                {
                    item = expiredItem;
                    return true;
                });

        _storage.Setup(s => s.TryGetWithoutRefresh(testKey, out It.Ref<CacheItem<string>>.IsAny))
                .Returns((string _, out CacheItem<string> item) =>
                {
                    item = expiredItem;
                    return true;
                });

        // Wait for the item to expire
        await Task.Delay(10);

        // Trigger expiration through cache access
        cache.TryGet(testKey, out _, out _);

        var raised = await eventRaised.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(raised, "Expired item access did not raise event");
    }

    [Fact]
    public void Dispose_PreventsFurtherOperations()
    {
        _cache.Dispose();
        _cache.Dispose();

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
        _storage.Setup(s => s.TryGetWithoutRefresh(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
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

    private static void VerifyOperationTimeout(LruCache<string, string> cache)
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
    public async Task TryGet_SchedulesExpiredItemRemoval()
    {
        var expiredItem = new CacheItem<string>("value", DateTime.UtcNow.AddMilliseconds(50));
        var removalCalled = new TaskCompletionSource<bool>();

        _storage.Setup(s => s.TryGet(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
                .Returns((string _, out CacheItem<string> item) =>
                {
                    item = expiredItem;
                    return true;
                });

        _storage.Setup(s => s.TryGetWithoutRefresh(It.IsAny<string>(), out It.Ref<CacheItem<string>>.IsAny))
                .Returns((string _, out CacheItem<string> item) =>
                {
                    item = expiredItem;
                    return true;
                });

        _storage.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback(() => removalCalled.SetResult(true));

        await Task.Delay(51);
        _cache.TryGet("key", out _, out _);

        var removed = await removalCalled.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(removed);
    }

    /// <summary>
    /// Tests for thread-safety and concurrency
    /// </summary>
    [Collection("LruCache Tests")]
    public class LruCacheConcurrencyTests
    {
        private readonly Mock<ILruStorage<string, string>> _storage;
        private readonly LruCache<string, string> _cache;
        private const int DefaultLockTimeoutMs = 100;
        private const int DefaultOperationTimeoutMs = 5000;

        public LruCacheConcurrencyTests()
        {
            _storage = new Mock<ILruStorage<string, string>>();
            Mock<ICacheStats> stats = new();
            ILruPolicy policy = new LruPolicy(TtlPolicy.None);

            _cache = new LruCache<string, string>(
                _storage.Object,
                policy,
                stats.Object,
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
        public async Task ConcurrentOperations_RespectLockingRules()
        {
            const int readerCount = 5;
            using var readersStarted = new CountdownEvent(readerCount);
            using var readersCompleted = new CountdownEvent(readerCount);
            using var startOperations = new ManualResetEventSlim();
            using var writeBlocked = new ManualResetEventSlim();
            using var completeWrite = new ManualResetEventSlim();
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(DefaultOperationTimeoutMs));

            var readerResults = new ConcurrentBag<bool>();
            Exception? asyncException = null;

            try 
            {
                SetupConcurrentOperationsStorage(writeBlocked, completeWrite);

                // Start concurrent readers with verification they've all started
                var readers = Enumerable.Range(0, readerCount)
                                        .Select(_ => Task.Run(() =>
                                        {
                                            try 
                                            {
                                                readersStarted.Signal();
                                                if (!startOperations.Wait(DefaultOperationTimeoutMs, cts.Token))
                                                {
                                                    throw new TimeoutException("Reader wait for start signal timed out");
                                                }

                                                var success = _cache.TryGet("key", out var _, out var error);
                                                readerResults.Add(success);

                                                readersCompleted.Signal();
                                                return (success, error);
                                            }
                                            catch (Exception ex)
                                            {
                                                asyncException = ex;
                                                throw;
                                            }
                                        }, cts.Token))
                                        .ToArray();

                // Ensure all readers are ready before starting writer
                if (!readersStarted.Wait(DefaultOperationTimeoutMs, cts.Token))
                {
                    throw new TimeoutException("Not all readers started within timeout");
                }

                var writer = Task.Run(() =>
                {
                    try 
                    {
                        if (!startOperations.Wait(DefaultOperationTimeoutMs, cts.Token))
                        {
                            throw new TimeoutException("Writer wait for start signal timed out");
                        }
                        return _cache.Put("key2", "value2", null, out _);
                    }
                    catch (Exception ex)
                    {
                        asyncException = ex;
                        throw;
                    }
                }, cts.Token);

                // Start all operations simultaneously
                startOperations.Set();

                // Wait for all readers to complete and verify writer is blocked
                if (!readersCompleted.Wait(DefaultOperationTimeoutMs, cts.Token))
                {
                    throw new TimeoutException("Readers failed to complete within timeout");
                }

                if (!writeBlocked.Wait(DefaultOperationTimeoutMs, cts.Token))
                {
                    throw new TimeoutException("Write operation not blocked within timeout");
                }

                // Allow writer to complete
                completeWrite.Set();

                // Wait for all operations with timeout
                var allTasks = new List<Task>(readers) { writer };
                await Task.WhenAll(allTasks).WaitAsync(TimeSpan.FromMilliseconds(DefaultOperationTimeoutMs), cts.Token);

                // If we got an async exception, throw it
                if (asyncException != null)
                {
                    throw new Exception("Async operation failed", asyncException);
                }

                // Verify results
                Assert.All(readerResults, success => Assert.True(success, "Reader operation failed"));
                Assert.True(await writer, "Writer operation failed");
            }
            finally
            {
                // Ensure we clean up even if test fails
                try { completeWrite.Set(); } catch { }
                try { startOperations.Set(); } catch { }
                cts.Cancel(); // Signal all tasks to stop
            }
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
}