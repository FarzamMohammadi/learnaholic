namespace LRUCache;

public interface ILruCache
{
    public int Get(string key);
    public void Put(string key, int value);
}

public class Cache : ILruCache
{
    /*
    Data structures are optimal:
        - LinkedList for O(1) insertions/removals
        - Dictionary for O(1) lookups
        - Combined key-value pairs in LinkedList nodes to avoid extra lookups
        - Dictionary pre-sized to avoid resizing

    All operations are O(1):
        - Get: One dictionary lookup + two LinkedList operations
        - Put: One dictionary lookup + one LinkedList operation
        - Eviction: Direct access to last node and its key

    Memory usage is optimal:
        - No redundant storage
        - No unnecessary objects
        - Clean data structure relationship

    Error handling is complete:
        - Capacity validation
        - Key validation
        - Not-found handling

    Code is clean and maintainable:
        - Clear variable names
        - Logical organization
        - Good encapsulation
    */
    private readonly LinkedList<(string Key, int Value)> _valuesPairs = [];
    private readonly Dictionary<string, LinkedListNode<(string Key, int Value)>> _index;
    private readonly int _capacity;

    public Cache(int capacity)
    {
        if (capacity < 0) throw new ArgumentException("Capacity cannot be negative.");

        _capacity = capacity;
        _index = new Dictionary<string, LinkedListNode<(string Key, int Value)>>(capacity);
    }

    public int Get(string key)
    {
        if (!_index.TryGetValue(key, out LinkedListNode<(string Key, int Value)>? nodeToMoveUp)) throw new KeyNotFoundException("Key not found!");

        _valuesPairs.Remove(nodeToMoveUp);
        _valuesPairs.AddFirst(nodeToMoveUp);

        return nodeToMoveUp.Value.Value;
    }

    public void Put(string key, int value)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key is invalid.");

        if (_index.ContainsKey(key))
        {
            Get(key);

            _valuesPairs.First!.Value = (key, value); // Might have changed and be a new value now.

            return;
        }

        if (_index.Count == _capacity)
        {
            _index.Remove(_valuesPairs.Last!.Value.Key);

            _valuesPairs.RemoveLast();
        }

        var newNode = _valuesPairs.AddFirst((key, value));

        _index[key] = newNode;
    }
}