namespace EnhancedLRUCache.Caching.Core;

public enum TtlPolicy
{
    Absolute = 1, // Items expire at a specific datetime
    Sliding = 2   // Items expire after period of non-use
}

public interface IEvictionPolicy
{
    public TimeSpan? DefaultTtl { get; }

    public bool UsesAbsoluteExpiration { get; }
    public bool UsesSlidingExpiration { get; }
}

public class EvictionPolicy
(
    TtlPolicy policy,
    TimeSpan? defaultTtl = null
) : IEvictionPolicy
{
    public TimeSpan? DefaultTtl { get; } = defaultTtl;

    public bool UsesAbsoluteExpiration => policy.HasFlag(TtlPolicy.Absolute);
    public bool UsesSlidingExpiration => policy.HasFlag(TtlPolicy.Sliding);
}