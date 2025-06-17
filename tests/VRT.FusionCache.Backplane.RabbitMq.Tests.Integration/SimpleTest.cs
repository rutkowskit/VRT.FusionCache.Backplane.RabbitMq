using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion.DangerZone;

namespace VRT.FusionCache.Backplane.RabbitMq.Tests.Integration;

public sealed class SimpleTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Test1()
    {
        // Arrange
        var provider1 = CreateTestServiceProvider("Cache1");
        var provider2 = CreateTestServiceProvider("Cache2");
        var cache1 = provider1.GetRequiredService<IFusionCache>();
        var cache2 = provider2.GetRequiredService<IFusionCache>();
        const string ExpectedValue = "value1";
        const string CacheKey = "key1";

        // Act                
        testOutputHelper.WriteLine("Setting value in cache1...");
        await cache2.SetAsync(CacheKey, "some value", token: TestContext.Current.CancellationToken);
        // delay to ensure the message is propagated
        await Task.Delay(100, cancellationToken: TestContext.Current.CancellationToken);
        testOutputHelper.WriteLine("Setting expected value in cache2...");
        await cache1.SetAsync(CacheKey, ExpectedValue, token: TestContext.Current.CancellationToken);
        await Task.Delay(500, TestContext.Current.CancellationToken); // Wait for the backplane to propagate the message        
        testOutputHelper.WriteLine("Getting value from cache2...");
        var value2 = await cache2.GetOrDefaultAsync<string>(CacheKey, token: TestContext.Current.CancellationToken);

        // Assert        
        value2.ShouldBe(ExpectedValue, "Value in cache2 should be sync when the value in cache1 i set");
    }

    private ServiceProvider CreateTestServiceProvider(string instanceId, string exchangeName = "test.fusioncache.exchange")
    {
        var result = new ServiceCollection();
        result.AddRabbitMqBackplane(opt =>
        {
            opt.ExchangeName = exchangeName;
            opt.RabbitMq.HostName = "localhost";
        });

        // Share the same distributed cache instance across tests
        result.AddSingleton(TestsShared.Services.GetRequiredService<IDistributedCache>());

        result
            .AddFusionCache()
            .WithRegisteredDistributedCache(false)
            .WithRegisteredBackplane()
            .WithSystemTextJsonSerializer()
            .WithOptions(opt =>
            {
                opt.SetInstanceId(instanceId);
                opt.WaitForInitialBackplaneSubscribe = true;
                opt.DefaultEntryOptions.Duration = TimeSpan.FromMinutes(2);
            });

        result.AddSingleton<ILoggerProvider>(new XUnitLoggerProvider(testOutputHelper, appendScope: false));
        result.AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace));

        return result.BuildServiceProvider();
    }
}
