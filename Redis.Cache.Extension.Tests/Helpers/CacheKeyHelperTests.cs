using Redis.Cache.Extension.Helpers;

namespace Redis.Cache.Extension.Tests.Helpers;
public class CacheKeyHelperTests
{
    [Fact]
    public void GetCacheKey_HandlesNullArguments()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey(null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetCacheKey_HandlesEmptyArguments()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetCacheKey_HandlesNullUrl()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey(null, "body", "method");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("-1252252501", result);
    }

    [Fact]
    public void GetCacheKey_HandlesValidUrl()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey("invalidUrl", null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1009418242", result);
    }

    [Fact]
    public void GetCacheKey_HandlesInvalidUrl()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey("https://www.someurl.com/api/v1/what", null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("api|v1|what|-1576854064", result);
    }

    [Fact]
    public void GetCacheKey_HandlesNullBody()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey("url", null, "method");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2026108198", result);
    }

    [Fact]
    public void GetCacheKey_HandlesNullMethod()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey("url", "body", null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("58995307", result);
    }

    [Fact]
    public void GetCacheKey_HandlesAllArguments()
    {
        // Act
        var result = CacheKeyHelper.GetCacheKey("url", "body", "method");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("-1821638794", result);
    }
}