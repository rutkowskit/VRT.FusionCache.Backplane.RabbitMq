using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq.Tests.Integration;

public sealed class SimpleTest
{
    [Fact]
    public void Test1()
    {
        // Arrange
        var options = new RabbitMqBackplaneOptions
        {
            ExchangeName = "test.exchange",
            RabbitMq = new RabbitMqBackplaneOptions.RabbitMqOptions
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            }
        };
        var backplane = new RabbitMqBackplane(Options.Create(options));
        var cache = new Fusio
        // Act



        // Assert
        Assert.NotNull(backplane);
        Assert.Equal("test.exchange", backplane.ExchangeName);
    }

    private ServiceCollection CreateTestServiceProvider()
    {
        var result = new ServiceCollection();
        result.AddRabbitMqBackplane();
        result
            .AddFusionCache()
            .WithRegisteredBackplane();
        return result.

    }
}
