namespace EnhancedLRUCache;

public interface ILruStorage<TKey, TValue>
{
    public bool TryGet(TKey key, out TValue value);
    public bool Add(TKey key, TValue value);
    public void Remove(TKey key);
    public void Clear();
}

public class LruStorage<TKey, TValue> : ILruStorage<TKey, TValue> where TKey : notnull
{
    public bool TryGet(TKey key, out TValue value)
    {
        throw new NotImplementedException();
    }

    public bool Add(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }

    public void Remove(TKey key)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }
}