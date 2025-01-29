using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace Redis.Cache.Extension.Tests;

[ExcludeFromCodeCoverage]
public abstract class TestBase<T> : IClassFixture<CommonFixture> where T : class
{
    private readonly CommonFixture _commonFixture;

    protected IFixture Fixture = new Fixture().Customize(new AutoMoqCustomization());
    protected T Sut => Fixture.Create<T>();

    public TestBase(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    public void Dispose()
    {
    }
}
