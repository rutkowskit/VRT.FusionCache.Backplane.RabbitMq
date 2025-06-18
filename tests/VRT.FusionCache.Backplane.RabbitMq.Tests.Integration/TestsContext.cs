//[assembly: TestPipelineStartup(typeof(TestsStartup))]

using Microsoft.Extensions.Configuration;

namespace VRT.FusionCache.Backplane.RabbitMq.Tests.Integration;


internal static class TestsShared
{
    private static Lazy<IServiceProvider> _serviceProvider = new(CreateTestServiceProvider);
    public static IServiceProvider Services => _serviceProvider.Value;
    public static IServiceProvider CreateTestServiceProvider()
    {
        var result = new ServiceCollection();
        result
            .AddConfiguration()
            .AddDistributedMemoryCache();
        return result.BuildServiceProvider();
    }

    private static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
        services.AddSingleton<IConfiguration>(configuration);

        return services;
    }
}