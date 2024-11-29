namespace LRUCache;

public interface ILRUCache
{
    public int Get(string key);
    public void Put(string key, int value);
}

public class Cache(int capacity) : ILRUCache
{
    private readonly LinkedList<int> Values = new();
    private readonly Dictionary<string, LinkedListNode<int>> ValuesIndex = new();
    private Dictionary<int, string> ValuesOrder = new();

    public int Get(string key)
    {
        if (!ValuesIndex.TryGetValue(key, out LinkedListNode<int>? nodeToMoveUp)) throw new KeyNotFoundException("Key not found!");
        
        Values.Remove(nodeToMoveUp);
        Values.AddFirst(nodeToMoveUp);
        

        return nodeToMoveUp.Value;
    }

    public void Put(string key, int value)
    {
        if (ValuesIndex.ContainsKey(key))
        {
            Get(key);

            Values.First!.Value = value; // Might have changed and be a new value now.

            return;
        }

        if (ValuesIndex.Count == capacity)
        {
            LinkedListNode<int> nodeToRemove = Values.Last;
        }
    }
}