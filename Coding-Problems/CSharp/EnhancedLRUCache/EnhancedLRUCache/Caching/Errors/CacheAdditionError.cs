namespace EnhancedLRUCache.Errors;

public enum CacheAdditionError
{
    None = 0,
    MaxMemorySizeExceeded = 1,
    ThreadLockTimeout = 2,
    StorageError = 3
}