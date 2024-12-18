namespace EnhancedLRUCache.Errors;

public enum CacheRetrievalError
{
    None = 0,
    ThreadLockTimeout = 1,
    ItemNotFound = 2,
    ItemExpired = 3
}