namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;
internal sealed record RabbitMqInstance
{
    public string Id { get; } = Guid.NewGuid().ToString();
}
