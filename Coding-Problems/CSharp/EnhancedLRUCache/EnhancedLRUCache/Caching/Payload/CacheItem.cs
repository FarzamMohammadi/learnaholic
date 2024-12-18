using System.Runtime.InteropServices;

namespace EnhancedLRUCache.Caching.Payload;

public interface ICacheItem
{
    public bool IsExpired();
    public void RefreshLastAccessed();
    public DateTime GetExpirationTime();
    public bool HasExpiration { get; }
}

public class CacheItem<TValue> : ICacheItem
{
    public TValue? Value { get; }
    public long Size { get; }
    private DateTime? AbsoluteExpiration { get; }
    private TimeSpan? SlidingExpiration { get; }
    public DateTime LastAccessed { get; private set; }

    public bool HasExpiration => AbsoluteExpiration.HasValue || SlidingExpiration.HasValue;

    public CacheItem(
        TValue? value,
        DateTime? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null)
    {
        if (absoluteExpiration.HasValue && absoluteExpiration <= DateTime.UtcNow)
            throw new ArgumentOutOfRangeException(nameof(absoluteExpiration), "Absolute Expiration older than UTC now!");

        if (slidingExpiration.HasValue && slidingExpiration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(slidingExpiration), "Sliding expiration must be positive");

        Size = GetObjectSize(value);

        Value = value;
        AbsoluteExpiration = absoluteExpiration;
        SlidingExpiration = slidingExpiration;
        LastAccessed = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        if (!HasExpiration) return false;

        var now = DateTime.UtcNow;
        var expiration = GetExpirationTime();

        return now > expiration;
    }

    public void RefreshLastAccessed()
    {
        LastAccessed = DateTime.UtcNow;
    }

    public DateTime GetExpirationTime()
    {
        if (AbsoluteExpiration.HasValue) return AbsoluteExpiration.Value;

        if (SlidingExpiration.HasValue) return LastAccessed + SlidingExpiration.Value;

        throw new InvalidOperationException("Cache entry has no expiration time set (both absolute and sliding expiration are null)");
    }

    private static long GetObjectSize(TValue? obj)
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