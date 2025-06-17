namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;

internal interface IBusService : IBusSubscriberService, IBusPublisherService;
internal interface IBusSubscriberService : IBusEventSubscriberService;

internal interface IBusPublisherService
{
    public static readonly string InstanceId = Guid.NewGuid().ToString();
    public Task Publish(BackplaneMessage message) => Publish(message, CancellationToken.None);
    Task Publish(BackplaneMessage message, CancellationToken cancellationToken);
}

internal interface IBusEventSubscriberService
{
    public Task<IDisposable> SubscribeEvent(IMessageHandler<BackplaneMessage> handler)
        => SubscribeEvent(handler, CancellationToken.None);

    Task<IDisposable> SubscribeEvent(
        IMessageHandler<BackplaneMessage> handler,
        CancellationToken cancellationToken = default);
}
