using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using ZiggyCreatures.Caching.Fusion.DangerZone;
using ZiggyCreatures.Caching.Fusion.Events;

namespace VRT.FusionCache.Backplane.RabbitMq.Tests.Integration;

public sealed class FusionCacheSyncTest(ITestOutputHelper testOutputHelper)
{
    // You can run this test in a loop in `Test Explorer` to ensure that there is no false negative result
    [Fact]
    public async Task GetOrDefaultAsync_WhenCache1ValueIsUpdated_ShouldSetTheMemoryCacheValueInCache2()
    {
        // Arrange
        var provider1 = CreateTestServiceProvider("Cache1");
        var provider2 = CreateTestServiceProvider("Cache2");
        var cache1 = provider1.GetRequiredService<IFusionCache>();
        var cache2 = provider2.GetRequiredService<IFusionCache>();
        const string ExpectedValue = "value1";
        const string CacheKey = "FusionCacheSyncTest:GetOrDefaultAsync_key1";

        // Act
        // wait for the fistt backplane to be received
        await WaitForCacheValueUpdate(cache2.Events.Distributed, () =>
        {
            testOutputHelper.WriteLine("[TestFlow] Setting value in cache2...");
            return cache2.SetAsync(CacheKey, "some old value", token: TestContext.Current.CancellationToken);
        });

        // delay to ensure the message is propagated
        await Task.Delay(100, cancellationToken: TestContext.Current.CancellationToken);
        await WaitForCacheValueUpdate(cache2.Events.Memory, () =>
        {
            testOutputHelper.WriteLine("[TestFlow] Setting expected value in cache1...");
            return cache1.SetAsync(CacheKey, ExpectedValue, token: TestContext.Current.CancellationToken);
        });

        testOutputHelper.WriteLine("[TestFlow] Getting value from cache2...");
        var value2 = await cache2.GetOrDefaultAsync<string>(CacheKey, token: TestContext.Current.CancellationToken);

        // Assert        
        value2.ShouldBe(ExpectedValue, "Value in cache2 should be sync when the value in cache1 i set");
    }

    [Fact]
    public async Task GetOrDefaultAsync_WhenCache1ValueIsRemoved_ShouldRemoveValueFromMemoryInCache2()
    {
        // Arrange
        var provider1 = CreateTestServiceProvider("Cache1");
        var provider2 = CreateTestServiceProvider("Cache2");
        var cache1 = provider1.GetRequiredService<IFusionCache>();
        var cache2 = provider2.GetRequiredService<IFusionCache>();
        const string CacheKey = "FusionCacheSyncTest:GetOrDefaultAsync_key2";

        // Act
        // wait for the fistt backplane to be received
        await WaitForCacheValueUpdate(cache2.Events.Distributed, () =>
        {
            testOutputHelper.WriteLine("[TestFlow] Setting value in cache2...");
            return cache2.SetAsync(CacheKey, "some old value", token: TestContext.Current.CancellationToken);
        });

        // delay to ensure the message is propagated
        await Task.Delay(100, cancellationToken: TestContext.Current.CancellationToken);
        await WaitForCacheValueUpdate(cache2.Events.Memory, () =>
        {
            testOutputHelper.WriteLine("[TestFlow] Removing value from cache1...");
            return cache1.RemoveAsync(CacheKey, token: TestContext.Current.CancellationToken);
        });

        testOutputHelper.WriteLine("[TestFlow] Getting value from cache2...");
        var value2 = await cache2.GetOrDefaultAsync<string>(CacheKey, token: TestContext.Current.CancellationToken);

        // Assert        
        value2.ShouldBe(default, "Value in cache2 should be default(string) value");
    }

    [Fact]
    public async Task GetOrDefaultAsync_WhenTwoCachesWithSameDCHaveDifferentChannelName_ShouldNotChangeMemoryCacheInCache2()
    {
        // Arrange
        var provider1 = CreateTestServiceProvider("Cache1");
        var provider2 = CreateTestServiceProvider("Cache2", "OtherChannel");
        var cache1 = provider1.GetRequiredService<IFusionCache>();
        var cache2 = provider2.GetRequiredService<IFusionCache>();
        const string ExpectedValue = "value1";
        const string CacheKey = "FusionCacheSyncTest:GetOrDefaultAsync_key3";

        // Act
        // wait for the fistt backplane to be received
        await WaitForCacheValueUpdate(cache2.Events.Distributed, () =>
        {
            testOutputHelper.WriteLine("[TestFlow] Setting value in cache2...");
            return cache2.SetAsync(CacheKey, ExpectedValue, token: TestContext.Current.CancellationToken);
        });

        // delay to ensure the message is propagated
        await Task.Delay(100, cancellationToken: TestContext.Current.CancellationToken);
        await WaitForCacheValueUpdate(cache1.Events.Distributed, () =>
        {
            testOutputHelper.WriteLine("[TestFlow] Setting expected value in cache1...");
            return cache1.SetAsync(CacheKey, "Some updated value", token: TestContext.Current.CancellationToken);
        });

        testOutputHelper.WriteLine("[TestFlow] Getting value from cache2...");
        var value2 = await cache2.GetOrDefaultAsync<string>(CacheKey, token: TestContext.Current.CancellationToken);
        var value1 = await cache1.GetOrDefaultAsync<string>(CacheKey, token: TestContext.Current.CancellationToken);
        // Assert        

        value2.ShouldBe(ExpectedValue, "Value in cache2 should be sync when the value in cache1 i set");
        value1.ShouldNotBe(ExpectedValue);
    }

    private async Task WaitForCacheValueUpdate(
        FusionCacheCommonEventsHub hub,
        Func<ValueTask> taskToExecute)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(5000);

        var syncSemaphore = new SemaphoreSlim(1, 1);
        syncSemaphore.Wait(cts.Token);

        void Events_Set(object? sender, FusionCacheEntryEventArgs e) => syncSemaphore.Release();
        try
        {
            hub.Set += Events_Set;
            hub.Remove += Events_Set;
            await taskToExecute();
            syncSemaphore.Wait(cts.Token);
        }
        finally
        {
            hub.Set -= Events_Set;
            hub.Remove -= Events_Set;
            syncSemaphore.Release();
        }
        cts.Token.ThrowIfCancellationRequested();
    }


    private ServiceProvider CreateTestServiceProvider(string instanceId, string? channelPrefix = null)
    {
        var result = new ServiceCollection();
        var configuration = TestsShared.Services.GetRequiredService<IConfiguration>();

        result
            .AddRabbitMqBackplane(options =>
            {
                options.ExchangeName = "FusionCacheBackplane.events";
                options.RabbitMq.HostName = "localhost";
                options.RabbitMq.UserName = "admin";
                options.RabbitMq.Password = "some_secret_password";
            });

        result
            .AddRabbitMqBackplane(configuration.GetSection("RabbitMqBackplane").Bind)
            .AddSingleton(TestsShared.Services.GetRequiredService<IDistributedCache>());

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
                opt.BackplaneChannelPrefix = channelPrefix;
            });

        result.AddSingleton<ILoggerProvider>(new XUnitLoggerProvider(testOutputHelper, appendScope: false));
        var logLevel = configuration.GetValue("Logging:LogLevel:Default", LogLevel.Information);
        result.AddLogging(cfg => cfg.SetMinimumLevel(logLevel));
        return result.BuildServiceProvider();
    }
}
