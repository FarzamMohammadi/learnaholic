# Enhanced LRU Cache

A thread-safe, generic LRU (Least Recently Used) cache implementation with TTL support, multiple eviction policies, and comprehensive statistics tracking.

## Features

- Generic cache supporting any key-value pair types
- Flexible TTL (Time To Live) management
    - Absolute expiration (items expire at specific datetime)
    - Sliding expiration (items expire after period of non-use)
- Memory management
    - Maximum item count limit
    - Total memory size limit
- Comprehensive statistics tracking
    - Hit/miss ratios
    - Eviction counts
    - Memory usage
    - Total requests
- Thread-safe operations
    - Concurrent read support
    - Optimized write locking using ReaderWriterLockSlim
    - Background cleanup for expired items

## Core API

```csharp
bool Put(TKey key, TValue value, TimeSpan? ttl, out CacheAdditionError error);
bool TryGet(TKey key, out TValue? value, out CacheRetrievalError error);
bool Remove(TKey key, out TValue? value, out CacheRemovalError error);
bool Clear();
```

## Statistics Interface

```csharp
public interface ICacheStats
{
    long TotalRequests { get; }
    long CacheHits { get; }
    long CacheMisses { get; }
    double HitRatio { get; }
    long EvictionCount { get; }
    long ExpiredCount { get; }
    long CurrentItemCount { get; }
    long TotalMemoryBytes { get; }
}
```

## Advanced Features

- Event monitoring system for cache operations (hits, misses, evictions)
- Customizable eviction callbacks
- Background cleanup worker for expired items
- IDisposable implementation for proper resource cleanup

## Thread Safety

The implementation ensures thread safety through:
- Read/write lock separation for optimal performance
- Atomic operations for statistics updates
- Thread-safe background cleanup process
- Proper synchronization for eviction policies

## Usage Example

```csharp
// Create a new cache instance with default settings
var cache = new EnhancedLRUCache<string, string>();

// Add item with TTL
cache.Put("key", "value", TimeSpan.FromMinutes(5));

// Try to retrieve item
if (cache.TryGet("key", out var value))
{
    Console.WriteLine($"Found value: {value}");
}

// Get current statistics
ICacheStats stats = cache.GetStatistics();
Console.WriteLine($"Hit ratio: {stats.HitRatio:P2}");
```