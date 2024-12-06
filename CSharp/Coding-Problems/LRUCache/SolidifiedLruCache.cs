/*
Below is an advanced implementation of Cache.cs

This design now follows SOLID principles:
    - Single Responsibility: Each class has one job
    - Open/Closed: Easy to add new cache policies or storage implementations
    - Liskov Substitution: All implementations are interchangeable
    - Interface Segregation: Interfaces are focused and minimal
    - Dependency Inversion: High-level modules depend on abstractions
*/

namespace LRUCache;

public interface ICache<TKey, TValue>
{
    TValue Get(TKey key);
    void Put(TKey key, TValue value);
}

public interface ICacheStore<TKey, TValue>
{
    int Count { get; }
    bool TryGet(TKey key, out TValue value);
    void Add(TKey key, TValue value);
    void Remove(TKey key);
}

public interface ICachePolicy<TKey>
{
    void NotifyAccessed(TKey key);
    void NotifyAdded(TKey key);
    TKey GetEvictionCandidate();
}

public class SolidifiedLruCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
{
    private readonly ICacheStore<TKey, TValue> _store;
    private readonly ICachePolicy<TKey> _policy;
    private readonly int _capacity;

    public SolidifiedLruCache(int capacity, ICacheStore<TKey, TValue> store, ICachePolicy<TKey> policy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity); // OR use "if (capacity < 0)" for a simpler approach

        _capacity = capacity;
        _store = store;
        _policy = policy;
    }

    public TValue Get(TKey key)
    {
        if (!_store.TryGet(key, out TValue value)) throw new KeyNotFoundException($"Key: {key} not found!");

        _policy.NotifyAccessed(key);

        return value;
    }

    public void Put(TKey key, TValue value)
    {
        // Validation below could be swapped with a more comprehensive version that covers a wide variety of types
        if (key is string stringKey && string.IsNullOrEmpty(stringKey)) throw new ArgumentException("Key is invalid.");

        if (_store.TryGet(key, out _))
        {
            _store.Remove(key);

            _store.Add(key, value);

            _policy.NotifyAccessed(key);

            return;
        }

        if (_store.Count == _capacity)
        {
            var keyToEvict = _policy.GetEvictionCandidate();
            _store.Remove(keyToEvict);
        }

        _store.Add(key, value);
        _policy.NotifyAdded(key);
    }
}

public class LinkedListCacheStore<TKey, TValue>(int capacity)
    : ICacheStore<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _index = new(capacity);
    private readonly LinkedList<(TKey Key, TValue Value)> _pairs = [];

    public int Count => _index.Count;

    public bool TryGet(TKey key, out TValue value)
    {
        if (_index.TryGetValue(key, out var node))
        {
            value = node.Value.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        var newNode = _pairs.AddFirst((key, value));
        _index[key] = newNode;
    }

    public void Remove(TKey key)
    {
        if (!_index.TryGetValue(key, out var node)) return;

        _pairs.Remove(node);
        _index.Remove(key);
    }
}

public class LruCachePolicy<TKey>(int capacity)
    : ICachePolicy<TKey> where TKey : notnull
{
    private readonly LinkedList<TKey> _accessOrder = [];
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _nodes = new(capacity);

    public void NotifyAccessed(TKey key)
    {
        if (!_nodes.TryGetValue(key, out var node)) return;

        _accessOrder.Remove(node);
        _accessOrder.AddFirst(node);
    }

    public void NotifyAdded(TKey key)
    {
        var node = _accessOrder.AddFirst(key);

        _nodes[key] = node;
    }

    public TKey GetEvictionCandidate()
    {
        if (_accessOrder.Last is null) throw new InvalidOperationException("Cache is empty");

        return _accessOrder.Last.Value;
    }
}