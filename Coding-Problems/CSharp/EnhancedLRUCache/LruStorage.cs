using System.Runtime.InteropServices;

namespace EnhancedLRUCache;

public enum CacheAdditionErrorType
{
    ItemAlreadyExists = 1,
    MaxMemorySizeExceeded = 2
}

public interface ILruStorage<TKey, TValue>
{
    public bool TryGet(TKey key, out TValue value);
    public (bool success, CacheAdditionErrorType? error) Add(TKey key, TValue value);
    public void Remove(TKey key);
    public void Clear();

    public int Count { get; }
    public long CurrentMemorySize { get; }
    public bool IsFull { get; }
}

public class LruStorage<TKey, TValue> : ILruStorage<TKey, TValue> where TKey : notnull
{
    private readonly LinkedList<(TKey Key, TValue Value)> _store;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _index;

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

    public bool TryGet(TKey key, out TValue value)
    {
        ThrowIfAnyIsNull((key, nameof(key)));

        if (!_index.TryGetValue(key, out var node))
        {
            value = default!;
            return false;
        }

        _store.Remove(node);
        _store.AddFirst(node);

        value = node.Value.Value;

        return true;
    }

    public (bool success, CacheAdditionErrorType? error) Add(TKey key, TValue value)
    {
        ThrowIfAnyIsNull(
            (key, nameof(key)),
            (value, nameof(value))
        );

        var newNodeSize = GetObjectSize(value);

        if (newNodeSize + _currentMemorySize > _maximumMemorySize)
        {
            return (false, CacheAdditionErrorType.MaxMemorySizeExceeded);
        }

        if (_index.ContainsKey(key)) return (false, CacheAdditionErrorType.ItemAlreadyExists);

        _index[key] = new LinkedListNode<(TKey Key, TValue Value)>((key, value));
        _store.AddFirst((key, value));

        _currentMemorySize += newNodeSize;

        if (_store.Count < _maximumItemCount) return (true, null);

        Remove(_store.Last!.Value.Key);

        return (true, null);
    }

    public void Remove(TKey key)
    {
        ThrowIfAnyIsNull((key, nameof(key)));

        if (!_index.TryGetValue(key, out var value)) throw new KeyNotFoundException();

        _store.Remove(value);
        _index.Remove(key);

        _currentMemorySize -= GetObjectSize(value.Value.Value);
    }

    public void Clear()
    {
        _store.Clear();
        _index.Clear();

        _currentMemorySize = 0;
    }

    private static void ThrowIfAnyIsNull(params (object? value, string name)[] parameters)
    {
        foreach (var parameter in parameters)
        {
            ArgumentNullException.ThrowIfNull(parameter.value);
        }
    }

    private static long GetObjectSize(TValue obj)
    {
        if (obj is null) return 0;

        // Special case: strings contain:
        // - Object header (16 bytes)
        // - Length field (4 bytes - int)
        // - Character array (2 bytes × length)
        if (obj is string str)
        {
            const int objectHeaderSize = 16; // Base object overhead

            return objectHeaderSize + sizeof(int) + (sizeof(char) * str.Length);
            //     |____________|    |_________|     |_______________________|
            //     Object header     Length field        Character array
            //     (16 bytes)        (4 bytes)          (2 bytes × length)
        }

        /*
            Marshal.SizeOf() - Size Calculation for .NET Types

            SUPPORTED (Unmanaged Types - Not CLR Managed):
                1. Primitive Types:
                    - Numeric: int, long, float, double, decimal
                    - Other: bool, char, byte
                2. Value Types:
                    - Structs marked with [StructLayout] attribute
                    - Blittable structs (those containing only primitive types)
                3. System Types:
                    - IntPtr, UIntPtr
                    - Handles (SafeHandle derivatives)
                4. Interop Types:
                    - COM objects
                    - P/Invoke structures
                    - Unmanaged function pointers

            NOT SUPPORTED (Managed Types - CLR Managed):
                1. Reference Types:
                    - All classes (including custom classes)
                    - String (special case: use length * sizeof(char))
                    - Arrays and Collections
                    - Delegates and Events
                2. Complex Types:
                    - Interfaces
                    - Generics
                    - Anonymous types
                    - Dynamic objects
                3. Framework Types:
                    - Most .NET types (Exception, Object, etc.)
                    - LINQ types
                    - Task and async types

            NOTES:
                - Size calculations are exact for unmanaged types
                - For managed types, true size varies due to:
                    * Object overhead (header, method table pointer)
                    * Memory alignment/padding
                    * Reference handling
                    * Garbage collection considerations
                - The fallback size (24 bytes) is a minimum estimate for managed objects
                  on 64-bit systems (object header + reference)
        */
        try
        {
            // Attempt Marshal.SizeOf for unmanaged types
            return Marshal.SizeOf(obj);
        }
        catch (ArgumentException)
        {
            // Fallback for managed types
            // Minimum size: object header (16 bytes) + reference (8 bytes) on 64-bit
            return 24;
        }
    }
}