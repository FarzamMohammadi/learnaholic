using System.Drawing;
using EnhancedLRUCache.Caching.Payload;
using Xunit;
using static System.Threading.Thread;

namespace EnhancedLRUCacheTests;

public class CacheItemTests
{
    private readonly Random _random = new();
    private CacheItem<int> _cacheItem;

    public CacheItemTests()
    {
        _cacheItem = new CacheItem<int>(
            _random.Next(),
            DateTime.UtcNow.AddMinutes(30),
            TimeSpan.FromMinutes(20)
        );
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(30, false)]
    public void AbsoluteExpiration_IsValidatedUponClassInstantiation(int seconds, bool shouldThrow)
    {
        var action = () => new CacheItem<int>(_random.Next(), DateTime.UtcNow.AddSeconds(seconds));

        if (shouldThrow)
        {
            Assert.Throws<ArgumentOutOfRangeException>(action);
        }
        else
        {
            var exceptions = Record.Exception(action);
            Assert.Null(exceptions);
        }
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(30, false)]
    public void SlidingExpiration_IsValidatedUponClassInstantiation(int seconds, bool shouldThrow)
    {
        var action = () => new CacheItem<int>(_random.Next(), null, TimeSpan.FromSeconds(seconds));

        if (shouldThrow)
        {
            Assert.Throws<ArgumentOutOfRangeException>(action);
        }
        else
        {
            var exceptions = Record.Exception(action);
            Assert.Null(exceptions);
        }
    }

    public static TheoryData<DateTime, bool> AbsoluteExpirationValidationData =>
        new()
        {
            { DateTime.UtcNow.AddSeconds(1), true },
            { DateTime.UtcNow.AddHours(1), false }
        };

    [Theory, MemberData(nameof(AbsoluteExpirationValidationData))]
    public void IsExpired_AccountsForAbsoluteExpiration(DateTime absExp, bool expected)
    {
        _cacheItem = new CacheItem<int>(_random.Next(), absExp);

        if (expected) Sleep(TimeSpan.FromSeconds(1));

        Assert.Equal(expected, _cacheItem.IsExpired());
    }

    [Fact]
    public void RefreshLastAccessed_UpdatesAccessTime()
    {
        var outdatedAccessedTime = _cacheItem.LastAccessed;

        _cacheItem.RefreshLastAccessed();

        var lastAccessedUpdated = _cacheItem.LastAccessed > outdatedAccessedTime;

        Assert.True(lastAccessedUpdated);
    }

    [Theory]
    [InlineData(true, false)]  // Only absolute expiration
    [InlineData(false, true)]  // Only sliding expiration
    [InlineData(false, false)] // No expiration - should throw
    public void GetExpirationTime_HandlesExpirationTypesCorrectly(bool useAbsolute, bool useSliding)
    {
        DateTime? absoluteExp = useAbsolute ? DateTime.UtcNow.AddMinutes(30) : null;
        TimeSpan? slidingExp = useSliding ? TimeSpan.FromMinutes(15) : null;
        var cacheItem = new CacheItem<int>(42, absoluteExp, slidingExp);

        if (!useAbsolute && !useSliding)
        {
            Assert.Throws<InvalidOperationException>(() => cacheItem.GetExpirationTime());
            return;
        }

        var expTime = cacheItem.GetExpirationTime();

        if (useAbsolute)
        {
            Assert.Equal(absoluteExp!.Value, expTime);
        }
        else
        {
            Assert.Equal(cacheItem.LastAccessed + slidingExp!.Value, expTime);
        }
    }

    [Theory]
    [InlineData("", 20)]    // Empty string: 16 (header) + 4 (length)
    [InlineData("abc", 26)] // String: 16 + 4 + (2 * 3)
    [InlineData(42, 4)]     // int is 4 bytes
    [InlineData(null, 0)]   // null value
    public void GetObjectSize_ReturnsCorrectSize<T>(T value, long expectedSize)
    {
        var cacheItem = new CacheItem<T>(value);

        Assert.Equal(expectedSize, cacheItem.Size);
    }

    [Fact]
    public void GetObjectSize_ComplexAndStructTypes()
    {
        // Complex object uses fallback
        var complexObject = new { Name = "Test", Value = 42 };
        var complexItem = new CacheItem<object>(complexObject);
        Assert.Equal(24, complexItem.Size);

        // Struct uses Marshal.SizeOf
        var point = new Point { X = 1, Y = 2 };
        var structItem = new CacheItem<Point>(point);
        Assert.Equal(8, structItem.Size);
    }
}