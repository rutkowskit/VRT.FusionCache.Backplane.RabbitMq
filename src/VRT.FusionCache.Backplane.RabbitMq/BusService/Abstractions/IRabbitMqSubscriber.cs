namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;

internal interface IRabbitMqSubscriber<T>
{
    Task Subscribe(CancellationToken cancellationToke = default);
    Task Unsubscribe(CancellationToken cancellationToke = default);
    bool IsConnected();
}