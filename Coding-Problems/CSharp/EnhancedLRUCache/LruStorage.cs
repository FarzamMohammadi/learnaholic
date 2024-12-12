using System.Runtime.InteropServices;

namespace EnhancedLRUCache;

public enum CacheAdditionErrorType
{
    MaxMemorySizeExceeded = 1
}

public interface ILruStorage<TKey, TValue>
{
    public bool ContainsKey(TKey key);
    public bool TryGet(TKey key, out CacheItem<TValue> cacheItem);
    public (bool success, CacheAdditionErrorType? error) TryPut(TKey key, CacheItem<TValue> cacheItem);
    public void Remove(TKey key);
    public void Clear();
    public IReadOnlyCollection<TKey> GetExpiredKeys();

    public int Count { get; }
    public long CurrentMemorySize { get; }
    public bool IsFull { get; }
}

public class LruStorage<TKey, TValue> : ILruStorage<TKey, TValue> where TKey : notnull
{
    private readonly LinkedList<(TKey Key, CacheItem<TValue> CacheItem)> _store;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, CacheItem<TValue> CacheItem)>> _index;

    private long _currentMemorySize;
    private readonly long _maximumMemorySize;
    private readonly int _maximumItemCount;

    public LruStorage(int maxItemCount, long maxMemorySize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxItemCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxMemorySize);

        _maximumMemorySize = maxMemorySize;
        _maximumItemCount = maxItemCount;

        _store = new();
        _index = new(maxItemCount);
    }

    public int Count => _store.Count;
    public long CurrentMemorySize => _currentMemorySize;
    public bool IsFull => _store.Count >= _maximumItemCount;

    public bool ContainsKey(TKey key) => _index.ContainsKey(key);

    public bool TryGet(TKey key, out CacheItem<TValue> cacheItem)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_index.TryGetValue(key, out var kvp))
        {
            cacheItem = default!;
            return false;
        }

        _store.Remove(kvp);
        _store.AddFirst(kvp);

        cacheItem = kvp.Value.CacheItem;

        cacheItem.RefreshLastAccessed();

        return true;
    }

    public (bool success, CacheAdditionErrorType? error) TryPut(TKey key, CacheItem<TValue> cacheItem)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(cacheItem);

        if (cacheItem.Size + _currentMemorySize > _maximumMemorySize)
        {
            return (false, CacheAdditionErrorType.MaxMemorySizeExceeded);
        }

        if (_index.ContainsKey(key))
        {
            RefreshCacheItem(key, cacheItem);
            return (true, null);
        }

        _index[key] = new LinkedListNode<(TKey Key, CacheItem<TValue> CacheItem)>((key, cacheItem));
        _store.AddFirst((key, cacheItem));

        _currentMemorySize += cacheItem.Size;

        if (_store.Count < _maximumItemCount) return (true, null);

        Remove(_store.Last!.Value.Key);

        return (true, null);
    }

    private void RefreshCacheItem(TKey key, CacheItem<TValue> cacheItem)
    {
        var newEntry = new LinkedListNode<(TKey Key, CacheItem<TValue> CacheItem)>((key, cacheItem));
        var oldEntry = _index[key];

        _store.Remove(oldEntry);
        _currentMemorySize -= oldEntry.Value.CacheItem.Size;

        _store.AddFirst(newEntry);
        _currentMemorySize += newEntry.Value.CacheItem.Size;

        _index[key] = newEntry;
    }

    public void Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_index.TryGetValue(key, out var kvp)) throw new KeyNotFoundException();

        _store.Remove(kvp);
        _index.Remove(key);

        _currentMemorySize -= kvp.Value.CacheItem.Size;
    }

    public void Clear()
    {
        _store.Clear();
        _index.Clear();

        _currentMemorySize = 0;
    }

    public IReadOnlyCollection<TKey> GetExpiredKeys()
    {
        return _store.Where(entry => entry.CacheItem.IsExpired())
            .Select(cacheItem => cacheItem.Key)
            .ToList()
            .AsReadOnly();
    }
}