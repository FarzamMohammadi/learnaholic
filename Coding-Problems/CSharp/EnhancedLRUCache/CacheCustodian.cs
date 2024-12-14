using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;

namespace EnhancedLRUCache;

public interface ICacheCustodian<TKey, TValue> : IDisposable
{
    public void Start();

    /// <summary>
    /// Temporarily stops custodian's cleanup task.
    /// <remarks>Cleanup can be resumed with `Start()`.</remarks>
    /// </summary>
    public void Stop();

    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemExpired;
}

public class CacheCustodian<TKey, TValue>
(
    TimeSpan cleanupInterval,
    TimeSpan? cleanupFailureRetryInterval,
    ILruCache<TKey, TValue> cache
) : ICacheCustodian<TKey, TValue>
    where TKey : notnull
{
    private Timer? _timer;

    private readonly TimeSpan _cleanupInterval = cleanupInterval <= TimeSpan.Zero
        ? throw new ArgumentOutOfRangeException(nameof(cleanupInterval))
        : cleanupInterval;

    private readonly TimeSpan? _cleanupFailureRetryInterval = cleanupFailureRetryInterval is { } interval
                                                              && (interval <= TimeSpan.Zero || interval > cleanupInterval)
        ? throw new ArgumentOutOfRangeException(nameof(cleanupFailureRetryInterval))
        : cleanupFailureRetryInterval;

    private readonly ILruCache<TKey, TValue> _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemExpired;

    public void Start()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (_timer?.Change(_cleanupInterval, _cleanupInterval) ?? false) return;

            _timer?.Dispose();
            _timer = new Timer(_ => RemoveExpiredCacheEntries(), null, _cleanupInterval, _cleanupInterval);
        }
    }

    private void RemoveExpiredCacheEntries()
    {
        if (_disposed) return;

        lock (_lock)
        {
            try
            {
                const int maxRetries = 3;
                var keyFetchAttempt = 0;

                IReadOnlyCollection<TKey> expiredKeys = [];

                while (keyFetchAttempt < maxRetries)
                {
                    expiredKeys = _cache.GetExpiredKeys(out var error);

                    if (error is CacheRetrievalError.None) break;

                    keyFetchAttempt++;

                    // If no retry interval specified, exit immediately to avoid contention
                    if (!_cleanupFailureRetryInterval.HasValue) return;

                    Thread.Sleep(_cleanupFailureRetryInterval.Value);
                }

                foreach (var key in expiredKeys)
                {
                    var successfullyRemoved = _cache.Remove(key, out var value, out _);

                    if (successfullyRemoved)
                    {
                        OnItemExpired(key, value);
                        continue;
                    }

                    // Same here - exit immediately if no retry interval
                    if (!_cleanupFailureRetryInterval.HasValue) return;

                    Thread.Sleep(_cleanupFailureRetryInterval.Value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Oh no!! Found a boo-boo in CacheCustodian during cleanup!\n" +
                    $"Exception: {e.Message}"
                );
            }
        }
    }

    private void OnItemExpired(TKey key, TValue? value) => ItemExpired?.Invoke(this, new CacheItemEventArgs<TKey, TValue>(key, value, DateTime.UtcNow));

    public void Stop()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            _timer?.Change(Timeout.Infinite, 0);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(CacheCustodian<TKey, TValue>));
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            lock (_lock)
            {
                Stop();
                _timer?.Dispose();
                _disposed = true;
            }
        }
    }
}