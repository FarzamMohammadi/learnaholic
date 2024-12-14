﻿using Xunit;

namespace EnhancedLRUCache.Tests;

public class LruPolicyTests
{
    [Fact]
    public void Constructor_WithNoTtl_ShouldSetPropertiesCorrectly()
    {
        var policy = new LruPolicy(TtlPolicy.Absolute);

        Assert.Null(policy.DefaultTtl);
        Assert.True(policy.UsesAbsoluteExpiration);
        Assert.False(policy.UsesSlidingExpiration);
    }

    [Fact]
    public void Constructor_WithTtl_ShouldSetPropertiesCorrectly()
    {
        var ttl = TimeSpan.FromMinutes(30);
        var policy = new LruPolicy(TtlPolicy.Sliding, ttl);

        Assert.Equal(ttl, policy.DefaultTtl);
        Assert.False(policy.UsesAbsoluteExpiration);
        Assert.True(policy.UsesSlidingExpiration);
    }

    [Theory]
    [InlineData(TtlPolicy.Absolute, true, false)]
    [InlineData(TtlPolicy.Sliding, false, true)]
    public void ExpirationFlags_ShouldReflectPolicy(TtlPolicy policy, bool expectedAbsolute, bool expectedSliding)
    {
        var lruPolicy = new LruPolicy(policy);

        Assert.Equal(expectedAbsolute, lruPolicy.UsesAbsoluteExpiration);
        Assert.Equal(expectedSliding, lruPolicy.UsesSlidingExpiration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(60)]
    public void DefaultTtl_ShouldHandleVariousTimeSpans(int? minutes)
    {
        TimeSpan? expectedTtl = minutes.HasValue ? TimeSpan.FromMinutes(minutes.Value) : null;
        var policy = new LruPolicy(TtlPolicy.Absolute, expectedTtl);

        Assert.Equal(expectedTtl, policy.DefaultTtl);
    }
}