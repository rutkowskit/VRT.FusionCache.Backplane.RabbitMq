namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;
internal interface IMessageHandler<T>
{
    public Task HandleMessageAsync(T message, CancellationToken cancellationToken);
}
