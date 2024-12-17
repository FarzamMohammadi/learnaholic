using EnhancedLRUCache.CacheItem;
using EnhancedLRUCache.Errors;

namespace EnhancedLRUCache;

public interface ILruStorage<TKey, TValue>
{
    public bool ContainsKey(TKey key);
    public bool TryGet(TKey key, out CacheItem<TValue> cacheItem);
    public bool TryPut(TKey key, CacheItem<TValue> cacheItem, out CacheAdditionError? error);
    public void Remove(TKey key);
    public void Clear();
    public IReadOnlyCollection<TKey> GetExpiredKeys();

    public int Count { get; }
    public long CurrentMemorySize { get; }
    public bool IsFull { get; }

    event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemEvicted;
}

public class LruStorage<TKey, TValue>
(
    int maxItemCount,
    long maxMemorySize,
    ICacheStats stats
)
    : ILruStorage<TKey, TValue> where TKey : notnull
{
    private readonly LinkedList<(TKey Key, CacheItem<TValue> CacheItem)> _store = new();
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, CacheItem<TValue> CacheItem)>> _index = new(maxItemCount);

    private readonly ICacheStats _stats = stats ?? throw new ArgumentNullException(nameof(stats));
    private readonly long _maximumMemorySize = maxMemorySize <= 0 ? throw new ArgumentOutOfRangeException(nameof(maxMemorySize)) : maxMemorySize;
    private readonly int _maximumItemCount = maxItemCount <= 0 ? throw new ArgumentOutOfRangeException(nameof(maxItemCount)) : maxItemCount;

    public int Count => _store.Count;
    public long CurrentMemorySize { get; private set; }
    public bool IsFull => Count >= _maximumItemCount || CurrentMemorySize >= _maximumMemorySize;

    public bool ContainsKey(TKey key) => _index.ContainsKey(key);

    public event EventHandler<CacheItemEventArgs<TKey, TValue>>? ItemEvicted;

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

    public bool TryPut(TKey key, CacheItem<TValue> cacheItem, out CacheAdditionError? error)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(cacheItem);

        if (cacheItem.Size > _maximumMemorySize)
        {
            error = CacheAdditionError.MaxMemorySizeExceeded;
            return false;
        }

        // Handle update case first - this actually frees memory
        if (_index.ContainsKey(key))
        {
            RefreshCacheItem(key, cacheItem);

            error = CacheAdditionError.None;
            return true;
        }

        // We prioritize new entry and evict items until enough space is available
        while (Count > 0 && cacheItem.Size + CurrentMemorySize > _maximumMemorySize) EvictLastEntry();

        // Add failsafe here in case our initial calculation was incorrect (after all, it's all estimation)
        if (cacheItem.Size + CurrentMemorySize > _maximumMemorySize)
        {
            error = CacheAdditionError.MaxMemorySizeExceeded;
            return false;
        }

        // Check if we need to evict for item count BEFORE adding
        if (_store.Count >= _maximumItemCount)
        {
            EvictLastEntry();
        }

        error = CacheAdditionError.None;

        var newNode = new LinkedListNode<(TKey Key, CacheItem<TValue> CacheItem)>((key, cacheItem));
        _index[key] = newNode;
        _store.AddFirst(newNode);
        CurrentMemorySize += cacheItem.Size;

        return true;
    }

    private void EvictLastEntry()
    {
        var evictionCandidate = _store.Last!.Value;

        Remove(evictionCandidate.Key);

        _stats.IncrementEvictionCount();

        OnItemEvicted(evictionCandidate.Key, evictionCandidate.CacheItem.Value);
    }

    private void RefreshCacheItem(TKey key, CacheItem<TValue> cacheItem)
    {
        var newEntry = new LinkedListNode<(TKey Key, CacheItem<TValue> CacheItem)>((key, cacheItem));
        var oldEntry = _index[key];

        _store.Remove(oldEntry);
        CurrentMemorySize -= oldEntry.Value.CacheItem.Size;

        _store.AddFirst(newEntry);
        CurrentMemorySize += newEntry.Value.CacheItem.Size;

        _index[key] = newEntry;
    }

    public void Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_index.TryGetValue(key, out var kvp)) return;

        _store.Remove(kvp);
        _index.Remove(key);

        CurrentMemorySize -= kvp.Value.CacheItem.Size;
    }

    public void Clear()
    {
        _store.Clear();
        _index.Clear();

        CurrentMemorySize = 0;
    }

    public IReadOnlyCollection<TKey> GetExpiredKeys()
    {
        return _store.Where(entry => entry.CacheItem.IsExpired())
                     .Select(cacheItem => cacheItem.Key)
                     .ToList()
                     .AsReadOnly();
    }

    private void OnItemEvicted(TKey key, TValue? value)
    {
        ItemEvicted?.Invoke(this, new CacheItemEventArgs<TKey, TValue>(key, value, DateTime.UtcNow));
    }
}