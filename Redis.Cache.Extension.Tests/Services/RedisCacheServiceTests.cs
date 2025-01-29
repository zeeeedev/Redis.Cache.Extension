using System.Text;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Moq;
using Redis.Cache.Extension.Models;
using Redis.Cache.Extension.Services;
using StackExchange.Redis;

namespace Redis.Cache.Extension.Tests.Services;

public class RedisCacheServiceTests : TestBase<RedisCacheService>
{
    private readonly Mock<IDatabase> _mockCache;
    private readonly CacheConfig _cacheConfig;
    private readonly string _applicationName = "App";
    private readonly string _anotherAplicationName = "AnotherApp";
    private readonly string _environmentName = "Test";
    private readonly string _validCacheKey = "validcachekey";
    private readonly string _validCacheKeyWithPrefix;
    private readonly string _validCacheKeyWithPrefix2;
    private readonly string _missingCacheKey = "missingcachekey";
    private readonly string _missingCacheKeyWithPrefix;
    private string _validCacheValue = "valid-cache-value";

    public RedisCacheServiceTests(CommonFixture commonFixture) : base(commonFixture)
    {
        _cacheConfig = Fixture.Freeze<CacheConfig>();
        _cacheConfig.IsEnabled = true;

        var mockEnvironment = Fixture.Freeze<Mock<IHostEnvironment>>();
        mockEnvironment.Setup(x => x.ApplicationName).Returns(_applicationName);
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(_environmentName);

        _mockCache = Fixture.Freeze<Mock<IDatabase>>();
        _validCacheKeyWithPrefix = $"{_applicationName}|{_environmentName}|{_validCacheKey}".ToUpperInvariant();
        _validCacheKeyWithPrefix2 = $"{_anotherAplicationName}|{_environmentName}|{_validCacheKey}".ToUpperInvariant();
        _missingCacheKeyWithPrefix = $"{_applicationName}|{_environmentName}|{_missingCacheKey}".ToUpperInvariant();
        _mockCache.Setup(x => x.StringGetAsync(new RedisKey(_missingCacheKeyWithPrefix), CommandFlags.None)).ReturnsAsync(new RedisValue());
        _mockCache.Setup(x => x.StringGetAsync(new RedisKey(_validCacheKeyWithPrefix), CommandFlags.None)).ReturnsAsync(_validCacheValue);
        _mockCache.Setup(x => x.StringGetAsync(new RedisKey(_validCacheKeyWithPrefix2), CommandFlags.None)).ReturnsAsync(_validCacheValue);
        _mockCache.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>())).Verifiable();
        _mockCache.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).Verifiable();
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
        _mockCache.Verify(x => x.StringSetAsync(string.Empty, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Never);
    }


    [Fact]
    public async Task RemoveAsync_CacheDisabled_DoesNotRemoveCachedItem()
    {
        // Arrange
        _cacheConfig.IsEnabled = false;

        // Act
        await Sut.RemoveAsync(string.Empty);

        // Assert
        _mockCache.Verify(x => x.KeyDeleteAsync(string.Empty, CommandFlags.FireAndForget), Times.Never);
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
    public async Task GetAsync_EmptyApplicationname_Returns_Null()
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
        _mockCache.Verify(x => x.StringSetAsync(string.Empty, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_EmptyKey_DoesNotRemoveCachedItem()
    {
        // Act
        await Sut.RemoveAsync(string.Empty);

        // Assert
        _mockCache.Verify(x => x.KeyDeleteAsync(string.Empty, CommandFlags.FireAndForget), Times.Never);
    }

    [Fact]
    public async Task GetAsync_MissingKey_Returns_Null()
    {
        // Act
        var result = await Sut.GetAsync<string>(_missingCacheKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_MissingKey_CreatesCacheEntry()
    {
        // Act
        await Sut.SetAsync(_missingCacheKey, string.Empty);

        // Assert
        _mockCache.Verify(x => x.StringSetAsync(new RedisKey(_missingCacheKeyWithPrefix), new RedisValue(string.Empty), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ValidKey_Returns_CachedItem()
    {
        // Act
        var result = await Sut.GetAsync<string>(_validCacheKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_validCacheValue, result);
    }

    [Fact]
    public async Task GetAsync_ValidKeyWithApplicaiontName_Returns_CachedItem()
    {
        // Act
        var result = await Sut.GetAsync<string>(_validCacheKey, _anotherAplicationName);

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
        _mockCache.Verify(x => x.StringSetAsync(_validCacheKeyWithPrefix, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Exactly(2));
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
        _mockCache.Verify(x => x.StringSetAsync(string.Empty, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_EmptyKeyList_DoesNotCreateCacheEntry()
    {
        // Act
        var keys = new List<string>() { };
        await Sut.RemoveAsync(keys);

        // Assert
        _mockCache.Verify(x => x.KeyDeleteAsync(string.Empty, CommandFlags.FireAndForget), Times.Never);
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
        _mockCache.Verify(x => x.StringSetAsync(_missingCacheKeyWithPrefix, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
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
        _mockCache.Verify(x => x.StringSetAsync(_missingCacheKeyWithPrefix, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), CommandFlags.FireAndForget), Times.Once);
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
    public async Task GetAsync_ValidKeyListWithApplicationName_Returns_CachedItem()
    {
        // Act
        var keys = new List<string>()
        {
            _validCacheKey
        };
        var result = await Sut.GetAsync<string>(keys, _anotherAplicationName);

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
        _mockCache.Verify(x => x.StringSetAsync(_validCacheKeyWithPrefix, It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), When.Always, CommandFlags.FireAndForget), Times.Exactly(2));
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
        _mockCache.Verify(x => x.KeyDeleteAsync(_validCacheKeyWithPrefix, CommandFlags.FireAndForget), Times.Once);
    }
}