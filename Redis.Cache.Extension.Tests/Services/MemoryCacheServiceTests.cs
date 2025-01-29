using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Moq;
using Redis.Cache.Extension.Models;
using Redis.Cache.Extension.Services;

namespace Redis.Cache.Extension.Tests.Services;

public class MemoryCacheServiceTests : TestBase<MemoryCacheService>
{
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ICacheEntry> _mockCacheEntry;
    private readonly CacheConfig _cacheConfig;
    private readonly string _missingCacheKey = "missing-cache-key";
    private readonly string _validCacheKey = "valid-cache-key";
    private object? _validCacheValue = "valid-cache-value";

    public MemoryCacheServiceTests(CommonFixture commonFixture) : base(commonFixture)
    {
        _cacheConfig = Fixture.Freeze<CacheConfig>();
        _cacheConfig.IsEnabled = true;

        var mockEnvironment = Fixture.Freeze<Mock<IHostEnvironment>>();
        mockEnvironment.Setup(x => x.ApplicationName).Returns("App");
        mockEnvironment.Setup(x => x.EnvironmentName).Returns("Test");

        _mockCacheEntry = Fixture.Freeze<Mock<ICacheEntry>>();
        _mockCacheEntry.Setup(x => x.SlidingExpiration).Verifiable();

        _mockCache = Fixture.Freeze<Mock<IMemoryCache>>();
        _mockCache.Setup(x => x.CreateEntry(_validCacheKey)).Returns(_mockCacheEntry.Object).Verifiable();
        _mockCache.Setup(x => x.TryGetValue(_validCacheKey, out _validCacheValue)).Returns(true);
        _mockCache.Setup(x => x.Remove(_validCacheKey)).Verifiable();
    }

    [Fact]
    public async Task GetAsync_CacheDisabled_Returns_Null()
    {
        // Arrange
        _cacheConfig.IsEnabled = false;

        // Act
        var result = await Sut.GetAsync<string>(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_CacheDisabled_DoesNotCreateCacheEntry()
    {
        // Arrange
        _cacheConfig.IsEnabled = false;

        // Act
        await Sut.SetAsync(string.Empty, string.Empty);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(string.Empty), Times.Never);
    }


    [Fact]
    public async Task RemoveAsync_CacheDisabled_DoesNotRemoveCachedItem()
    {
        // Arrange
        _cacheConfig.IsEnabled = false;

        // Act
        await Sut.RemoveAsync(string.Empty);

        // Assert
        _mockCache.Verify(x => x.Remove(string.Empty), Times.Never);
    }

    [Fact]
    public async Task GetAsync_EmptyKey_Returns_Null()
    {
        // Act
        var result = await Sut.GetAsync<string>(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_NullApplicationName_Returns_Null()
    {
        // Act
        var result = await Sut.GetAsync<string>(string.Empty, string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_EmptyKey_DoesNotAddCacheEntry()
    {
        // Act
        await Sut.SetAsync(string.Empty, _validCacheValue);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(string.Empty), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_EmptyKey_DoesNotRemoveCachedItem()
    {
        // Act
        await Sut.RemoveAsync(string.Empty);

        // Assert
        _mockCache.Verify(x => x.Remove(string.Empty), Times.Never);
    }

    [Fact]
    public async Task GetAsync_MissingKey_Returns_Null()
    {
        // Act
        var result = await Sut.GetAsync<object>(_missingCacheKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_MissingKey_CreatesCacheEntry()
    {
        // Act
        await Sut.SetAsync(_missingCacheKey, string.Empty);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(_missingCacheKey), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ValidKey_Returns_CachedItem()
    {
        // Act
        var result = await Sut.GetAsync<object>(_validCacheKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_validCacheValue, result);
    }

    [Fact]
    public async Task GetAsync_ValidKeyWithApplicationName_Returns_CachedItem()
    {
        // Act
        var result = await Sut.GetAsync<object>(_validCacheKey, "applicationname");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_validCacheValue, result);
    }

    [Fact]
    public async Task SetAsync_ExistingKey_UpdatesExistingCachedItem()
    {
        // Act
        await Sut.SetAsync(_validCacheKey, string.Empty);
        await Sut.SetAsync(_validCacheKey, _validCacheValue);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(_validCacheKey), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAsync_EmptyKeyList_Returns_Null()
    {
        // Act
        var keys = new List<string>() { };
        var result = await Sut.GetAsync<string>(keys);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_EmptyKeyList_DoesNotCreateCacheEntry()
    {
        // Act
        var keys = new List<string>() { };
        await Sut.SetAsync(keys, string.Empty);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(keys), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_EmptyKeyList_DoesNotCreateCacheEntry()
    {
        // Act
        var keys = new List<string>() { };
        await Sut.RemoveAsync(keys);

        // Assert
        _mockCache.Verify(x => x.Remove(string.Empty), Times.Never);
    }

    [Fact]
    public async Task GetAsync_MissingKeyList_Returns_Null()
    {
        // Act
        var keys = new List<string>()
        {
            _missingCacheKey
        };
        var result = await Sut.GetAsync<string>(keys);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_MissingKeyList_CreatesCachedEntry()
    {
        // Act
        var keys = new List<string>()
        {
            _missingCacheKey
        };
        await Sut.SetAsync(keys, string.Empty);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(_missingCacheKey), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithSlidingExpiration_CreatesCachedEntry()
    {
        // Arrange
        var slidingExpiration = new TimeSpan(0, 0, 1);

        // Act
        var keys = new List<string>()
        {
            _missingCacheKey
        };
        await Sut.SetAsync(keys, string.Empty, null, slidingExpiration);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(_missingCacheKey), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ValidKeyList_Returns_CachedItem()
    {
        // Act
        var keys = new List<string>()
        {
            _validCacheKey
        };
        var result = await Sut.GetAsync<string>(keys);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_validCacheValue, result);
    }

    [Fact]
    public async Task SetAsync_ValidKeyList_UpdatesCachedItem()
    {
        // Act
        var keys = new List<string>()
        {
            _validCacheKey
        };
        await Sut.SetAsync(keys, string.Empty);
        await Sut.SetAsync(keys, _validCacheValue);

        // Assert
        _mockCache.Verify(x => x.CreateEntry(_validCacheKey), Times.Exactly(2));
    }

    [Fact]
    public async Task RemoveAsync_ValidKeyList_RemovesCachedItem()
    {
        // Act
        var keys = new List<string>()
        {
            _validCacheKey
        };
        await Sut.RemoveAsync(keys);

        // Assert
        _mockCache.Verify(x => x.Remove(_validCacheKey), Times.Once);
    }
}