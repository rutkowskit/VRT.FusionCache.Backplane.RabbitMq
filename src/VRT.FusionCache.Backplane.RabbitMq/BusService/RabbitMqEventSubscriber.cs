namespace VRT.FusionCache.Backplane.RabbitMq.BusService;
internal sealed class RabbitMqEventSubscriber<T>(
    ConnectionFactory factory,
    IMessageHandler<T> handler)
    : BaseRabbitMqSubscriber<T>(factory, handler)
{
    protected override async Task<ConnectedChannelContext> ConnectToQueue(ChannelContext channel,
        CancellationToken cancellationToken)
    {
        var binding = await channel.Channel
            .ConnectToEvents(MessageTypeName, cancellationToken)
            .ConfigureAwait(false);

        return new ConnectedChannelContext(channel)
        {
            ExchangeName = binding.ExchangeName,
            QueueName = binding.QueueName
        };
    }
}