namespace EnhancedLRUCache.Errors;

public enum CacheRemovalError
{
    None = 0,
    ThreadLockTimeout = 1,
    ItemNotFound = 2,
    StorageError = 3
}