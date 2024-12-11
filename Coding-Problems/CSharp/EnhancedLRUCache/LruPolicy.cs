namespace EnhancedLRUCache;

[Flags]
public enum TtlPolicy
{
    None = 0,
    AbsoluteExpiration = 1 << 0, // 0001 - 1 - Items expire at a specific datetime
    SlidingExpiration = 1 << 1,  // 0010 - 2 - Items expire after period of non-use
    CacheLevelTtl = 1 << 2,      // 0100 - 4 - TTL settings applied at cache level
    ItemLevelTtl = 1 << 3        // 1000 - 8 - TTL settings applied at individual item level
}

public class LruPolicy
(
    TtlPolicy policy,
    TimeSpan? defaultTtl = null
)
{
    public TimeSpan? DefaultTtl { get; } = defaultTtl;

    public bool UsesAbsoluteExpiration => policy.HasFlag(TtlPolicy.AbsoluteExpiration);
    public bool UsesSlidingExpiration => policy.HasFlag(TtlPolicy.SlidingExpiration);
    public bool UsesCacheLevelTtl => policy.HasFlag(TtlPolicy.CacheLevelTtl);
    public bool UsesItemLevelTtl => policy.HasFlag(TtlPolicy.ItemLevelTtl);
}