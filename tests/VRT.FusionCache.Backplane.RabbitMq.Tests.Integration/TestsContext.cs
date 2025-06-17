//[assembly: TestPipelineStartup(typeof(TestsStartup))]

namespace VRT.FusionCache.Backplane.RabbitMq.Tests.Integration;



//internal class TestsStartup : ITestPipelineStartup
//{    
//    public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
//    {
//        throw new NotImplementedException();
//    }

//    public ValueTask StopAsync()
//    {
//        throw new NotImplementedException();
//    }
//}

internal static class TestsShared
{
    private static Lazy<IServiceProvider> _serviceProvider = new(CreateTestServiceProvider);
    public static IServiceProvider Services => _serviceProvider.Value;
    public static IServiceProvider CreateTestServiceProvider()
    {
        var result = new ServiceCollection();
        result.AddDistributedMemoryCache();
        return result.BuildServiceProvider();
    }
}