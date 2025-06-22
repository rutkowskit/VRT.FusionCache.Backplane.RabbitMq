namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;

internal interface IBusService : IBusSubscriberService, IBusPublisherService;
internal interface IBusSubscriberService : IBusEventSubscriberService;

internal interface IBusPublisherService
{
    Task Publish(BackplaneMessage message, string channelName, CancellationToken cancellationToken);
}

internal interface IBusEventSubscriberService
{
    Task<IDisposable> SubscribeEvent(
        IMessageHandler<BackplaneMessage> handler,
        string channelName,
        CancellationToken cancellationToken);
}
