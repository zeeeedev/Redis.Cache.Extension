using AutoFixture;
using Moq;
using Redis.Cache.Extension.Filters;
using Redis.Cache.Extension.Services;

namespace Redis.Cache.Extension.Tests.Filters;
public class CacheFilterTests : TestBase<CacheFilter>
{
    public CacheFilterTests(CommonFixture commonFixture) : base(commonFixture)
    {
        var mockCache = Fixture.Freeze<Mock<ICacheService>>();
        mockCache.Setup(x => x.GetAsync<string>(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(string.Empty).Verifiable();
        mockCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>())).Verifiable();
        mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>())).Verifiable();
    }

    // [Fact]
    public async Task OnActionExecutionAsync_HandlesSomething()
    {
        await Task.CompletedTask;
        // Coming soon
    }
}