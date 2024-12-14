using EnhancedLRUCache;
using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;
using Moq;
using Xunit;
using static System.Threading.Thread;

namespace EnhancedLRUCacheTests;

public class CacheCustodianTests
{
    private readonly Mock<ILruCache<string, int>> _cache = new();

    private readonly TimeSpan _validInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _shortInterval = TimeSpan.FromMilliseconds(100);
    private readonly TimeSpan _testTimeout = TimeSpan.FromMilliseconds(300);

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(30, false)]
    public void CleanupInterval_IsValidatedUponClassInstantiation(int seconds, bool shouldThrow)
    {
        var interval = TimeSpan.FromSeconds(seconds);

        AssertThrowsIfExpected(() => CreateCustodian(_cache.Object, cleanupInterval: interval), shouldThrow);
    }

    [Theory]
    [InlineData(30, 0, true)]
    [InlineData(30, -1, true)]
    [InlineData(30, 31, true)]
    [InlineData(30, 15, false)]
    public void CleanupFailureRetryInterval_IsValidatedUponClassInstantiation(int cleanupSeconds, int retrySeconds, bool shouldThrow)
    {
        var cleanupInterval = TimeSpan.FromSeconds(cleanupSeconds);
        var retryInterval = TimeSpan.FromSeconds(retrySeconds);

        AssertThrowsIfExpected(() => CreateCustodian(_cache.Object, cleanupInterval, retryInterval), shouldThrow);
    }

    [Fact]
    public void NullCache_Throws() => Assert.Throws<ArgumentNullException>(() => CreateCustodian(null));

    [Fact]
    public async Task Start_InitiatesCleanupCycle()
    {
        SetupExpiredKeys(["key1", "key2"]);

        var custodian = CreateCustodian(_cache.Object, _shortInterval);
        var itemExpiredCount = 0;
        custodian.ItemExpired += (_, _) => itemExpiredCount++;

        custodian.Start();
        await Task.Delay(_testTimeout);

        Assert.True(itemExpiredCount >= 2);
        _cache.Verify(c => c.GetExpiredKeys(out It.Ref<CacheRetrievalError>.IsAny), Times.AtLeast(2));
    }

    [Fact]
    public async Task Stop_PausesCleanupCycle()
    {
        var callCount = 0;
        var mre = new ManualResetEventSlim();

        _cache.Setup(c => c.GetExpiredKeys(out It.Ref<CacheRetrievalError>.IsAny))
            .Returns(["key1"])
            .Callback(() =>
            {
                callCount++;
                mre.Set();
            });

        var custodian = CreateCustodian(_cache.Object, _shortInterval);

        custodian.Start();

        Assert.True(mre.Wait(_testTimeout)); // Wait here until GetExpiredKeys is hit

        var callsBeforeStop = callCount;
        custodian.Stop();

        // Give enough time for any potential additional calls
        await Task.Delay(50); // Short delay since we're just checking no more calls happen

        Assert.Equal(callsBeforeStop, callCount);
        _cache.Verify(c => c.GetExpiredKeys(out It.Ref<CacheRetrievalError>.IsAny), Times.Exactly(callsBeforeStop));
    }

    [Fact]
    public void ItemExpired_EventContainsCorrectData()
    {
        const string testKey = "testKey";
        const int expectedValue = 42;
        SetupExpiredKeys([testKey], removeValue: expectedValue);

        var custodian = CreateCustodian(_cache.Object, _shortInterval);
        CacheItemEventArgs<string, int>? eventArgs = null;
        custodian.ItemExpired += (_, args) => eventArgs = args;

        custodian.Start();
        Sleep(_testTimeout);

        Assert.NotNull(eventArgs);
        Assert.Equal(testKey, eventArgs.Key);
        Assert.Equal(expectedValue, eventArgs.Value);
        Assert.True(eventArgs.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void MultipleStartCalls_DoNotCreateMultipleTimers()
    {
        var callCount = 0;
        var mre = new ManualResetEventSlim();

        _cache.Setup(c => c.GetExpiredKeys(out It.Ref<CacheRetrievalError>.IsAny))
            .Returns(["key1"])
            .Callback(() =>
            {
                callCount++;
                mre.Set();
            });

        var custodian = CreateCustodian(_cache.Object, _shortInterval);

        custodian.Start();
        custodian.Start();

        Assert.True(mre.Wait(_testTimeout));
        custodian.Stop();

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ConcurrentStartStop_HandledCorrectly()
    {
        var custodian = CreateCustodian(_cache.Object, _shortInterval);
        var tasks = Enumerable.Range(0, 10).Select(_ =>
            Task.Run(() =>
            {
                custodian.Start();
                Sleep(10);
                custodian.Stop();
            }));

        await Task.WhenAll(tasks); // Verifies that concurrent Start/Stop operations don't throw exceptions
    }

    [Fact]
    public void NoRetryInterval_ExitsImmediatelyOnGetExpiredKeysFailure()
    {
        var retrievalCount = 0;
        var mre = new ManualResetEventSlim();

        _cache.Setup(c => c.GetExpiredKeys(out It.Ref<CacheRetrievalError>.IsAny))
            .Returns((out CacheRetrievalError error) =>
            {
                retrievalCount++;
                error = CacheRetrievalError.ThreadLockTimeout;
                mre.Set(); // Signal that we've made first attempt
                return Array.Empty<string>();
            });

        var custodian = CreateCustodian(_cache.Object, _shortInterval); // No retry interval

        custodian.Start();

        Assert.True(mre.Wait(_testTimeout)); // Wait here until GetExpiredKeys is hit
        custodian.Stop();                    // Stop timer to prevent additional cycles

        Assert.Equal(1, retrievalCount); // Should exit after first failure without retry
    }

    [Fact]
    public void Dispose_PreventsFurtherOperations()
    {
        var custodian = CreateCustodian(_cache.Object);
        custodian.Dispose();

        Assert.Throws<ObjectDisposedException>(() => custodian.Start());
        Assert.Throws<ObjectDisposedException>(() => custodian.Stop());
    }

    private void SetupExpiredKeys(IReadOnlyCollection<string> keys, bool removeSucceeds = true, int? removeValue = default)
    {
        _cache.Setup(c => c.GetExpiredKeys(out It.Ref<CacheRetrievalError>.IsAny))
            .Returns(keys);

        _cache.Setup(c => c.Remove(It.IsAny<string>(), out It.Ref<int>.IsAny, out It.Ref<CacheRemovalError>.IsAny))
            .Returns((string _, out int value, out CacheRemovalError error) =>
            {
                value = removeValue ?? default;
                error = CacheRemovalError.None;
                return removeSucceeds;
            });
    }

    private CacheCustodian<string, int> CreateCustodian(
        ILruCache<string, int>? cache,
        TimeSpan? cleanupInterval = null,
        TimeSpan? retryInterval = null)
        => new(cleanupInterval ?? _validInterval, retryInterval, cache!);

    private static void AssertThrowsIfExpected(Action action, bool shouldThrow)
    {
        if (shouldThrow)
        {
            Assert.Throws<ArgumentOutOfRangeException>(action);
            return;
        }

        Assert.Null(Record.Exception(action));
    }
}