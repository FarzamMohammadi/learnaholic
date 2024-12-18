﻿using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;

namespace EnhancedLRUCache;

public interface ILruCache<TKey, TValue> : IDisposable
{
    public bool Put(TKey key, TValue? value, TimeSpan? ttl, out CacheAdditionError error);
    public bool TryGet(TKey key, out TValue? value, out CacheRetrievalError error);
    public bool Remove(TKey key, out TValue? value, out CacheRemovalError error);
    public IReadOnlyCollection<TKey> GetExpiredKeys(out CacheRetrievalError error);
    public bool Clear();

    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemExpired;
    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemEvicted;
}

public class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ILruStorage<TKey, TValue> _storage;
    private readonly CacheCustodian<TKey, TValue> _custodian;
    private readonly ILruPolicy _policy;
    private readonly ICacheStats _stats;

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly TimeSpan _lockTimeout;
    private bool _disposed;

    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemExpired;
    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemEvicted;

    public LruCache(
        ILruStorage<TKey, TValue> storage,
        ILruPolicy policy,
        ICacheStats stats,
        TimeSpan? lockTimeout = null,
        TimeSpan? cleanupInterval = null,
        TimeSpan? cleanupRetryInterval = null
    )
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));

        _lockTimeout = lockTimeout ?? TimeSpan.FromMinutes(3);

        _storage.ItemEvicted += OnStorageItemEvicted;

        _custodian = new CacheCustodian<TKey, TValue>(
            cleanupInterval ?? TimeSpan.FromMinutes(10),
            cleanupRetryInterval,
            this
        );

        _custodian.ItemExpired += OnCustodianItemExpired;
        _custodian.Start();
    }

    /*
         ReaderWriterLockSlim Class is used for protecting a resource that is read by multiple threads and written to by one thread at a time.
         https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-threading-readerwriterlockslim

         ReaderWriterLockSlim provides three locking modes:
             - Read Mode: Multiple threads can read concurrently
             - Write Mode: One thread gets exclusive access for modifications
             - Upgradeable Read: Special mode allowing a thread to upgrade to write mode without releasing its read lock

         The performance of ReaderWriterLockSlim is significantly better than ReaderWriterLock and should be preferred over it.

         Best Practices for Lock Management:
            - If you know you'll need to write, take a write lock immediately
            - Avoid lock upgrading (read -> write)
            - Keep the lock duration as short as possible
            - Always use try/finally for lock release
     */

    public bool Put(TKey key, TValue? value, TimeSpan? ttl, out CacheAdditionError error)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();

        var newCacheEntry = CreateCacheEntry(value, ttl);

        if (!_lock.TryEnterWriteLock(_lockTimeout))
        {
            error = CacheAdditionError.ThreadLockTimeout;
            return false;
        }

        try
        {
            _stats.IncrementRequestCount();

            var itemSuccessfullyAdded = _storage.TryPut(key, newCacheEntry, out var storageError);

            if (!itemSuccessfullyAdded)
            {
                error = storageError ?? CacheAdditionError.StorageError;
                return false;
            }

            _stats.UpdateItemCount(1);
            _stats.UpdateMemory(newCacheEntry.Size);

            error = CacheAdditionError.None;
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool TryGet(TKey key, out TValue? value, out CacheRetrievalError error)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();

        value = default;

        if (!_lock.TryEnterReadLock(_lockTimeout))
        {
            error = CacheRetrievalError.ThreadLockTimeout;
            return false;
        }

        try
        {
            _stats.IncrementRequestCount();

            if (!_storage.TryGet(key, out var cacheItem))
            {
                _stats.IncrementMissedRequestCount();

                error = CacheRetrievalError.ItemNotFound;
                return false;
            }

            if (cacheItem.IsExpired())
            {
                OnItemExpired(key, value);

                _stats.IncrementMissedRequestCount();
                _stats.IncrementExpiredCount();

                // Schedule async removal of expired item
                Task.Run(() => Remove(key, out _, out _));

                error = CacheRetrievalError.ItemExpired;
                return false;
            }

            value = cacheItem.Value;

            error = CacheRetrievalError.None;
            return true;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Remove(TKey key, out TValue? value, out CacheRemovalError error)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();

        value = default;

        if (!_lock.TryEnterWriteLock(_lockTimeout))
        {
            error = CacheRemovalError.ThreadLockTimeout;
            return false;
        }

        try
        {
            var found = _storage.TryGet(key, out var entry);

            if (!found)
            {
                error = CacheRemovalError.ItemNotFound;
                return false;
            }

            _storage.Remove(key);

            _stats.UpdateItemCount(-1);
            _stats.UpdateMemory(-entry.Size);

            value = entry.Value;
            error = CacheRemovalError.None;
            return true;
        }
        catch
        {
            error = CacheRemovalError.StorageError;
            return false;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IReadOnlyCollection<TKey> GetExpiredKeys(out CacheRetrievalError error)
    {
        if (!_lock.TryEnterReadLock(_lockTimeout))
        {
            error = CacheRetrievalError.ThreadLockTimeout;
            return [];
        }

        try
        {
            var expiredKeys = _storage.GetExpiredKeys();

            error = CacheRetrievalError.None;
            return expiredKeys;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Clear()
    {
        if (!_lock.TryEnterWriteLock(_lockTimeout)) return false;

        try
        {
            _storage.Clear();

            _stats.ClearMetrics();

            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private CacheItem<TValue> CreateCacheEntry(TValue? value, TimeSpan? ttl)
    {
        DateTime? absoluteExpiration = null;
        TimeSpan? slidingExpiration = null;

        if (_policy.UsesAbsoluteExpiration)
        {
            absoluteExpiration = DateTime.UtcNow + (ttl ?? _policy.DefaultTtl);
        }

        if (_policy.UsesSlidingExpiration)
        {
            slidingExpiration = ttl ?? _policy.DefaultTtl;
        }

        var newCacheEntry = new CacheItem<TValue>(value, absoluteExpiration, slidingExpiration);

        return newCacheEntry;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(LruCache<TKey, TValue>));
    }

    // Following the standard IDisposable pattern rather than a simple Dispose() implementation
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
            _lock.Dispose();
            _custodian.Dispose();
        }

        _disposed = true;
    }

    private void OnItemExpired(TKey key, TValue? value) => ItemExpired?.Invoke(this, new CacheItemEventArgs<TKey, TValue>(key, value, DateTime.UtcNow));

    private void OnCustodianItemExpired(object? sender, CacheItemEventArgs<TKey, TValue> item)
    {
        OnItemExpired(item.Key, item.Value);
    }

    private void OnStorageItemEvicted(object? sender, CacheItemEventArgs<TKey, TValue> item)
    {
        OnItemEvicted(item.Key, item.Value);
    }

    private void OnItemEvicted(TKey key, TValue? value) => ItemEvicted?.Invoke(this, new CacheItemEventArgs<TKey, TValue>(key, value, DateTime.UtcNow));
}