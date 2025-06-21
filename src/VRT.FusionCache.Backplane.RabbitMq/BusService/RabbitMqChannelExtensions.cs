using System.Text;

namespace VRT.FusionCache.Backplane.RabbitMq.BusService;
internal static class RabbitMqChannelExtensions
{
    internal record ExchangeBinding(IChannel Channel, string ExchangeName);
    internal record QueueExchangeBinding(IChannel Channel, string QueueName, string ExchangeName)
        : ExchangeBinding(Channel, ExchangeName);

    public static async Task PublishEvent(this ExchangeBinding binding,
        string message,
        string? routingKey = null,
        CancellationToken cancellationToken = default)
    {
        var body = CreateMessageBody(message);
        var basicProperties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Transient,
            Persistent = false,
            AppId = RabbitMqConstants.AppId,
            Type = typeof(BackplaneMessage).FullName ?? "UnknownType"
        };
        await binding.Channel.BasicPublishAsync(
            binding.ExchangeName, routingKey ?? "", false,
            basicProperties: basicProperties, body: body,
            cancellationToken: cancellationToken).AsTask().ConfigureAwait(false);
    }

    public static async Task<QueueExchangeBinding> ConnectToEvents(this IChannel channel,
        string typeName,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var exchange = await channel
            .DeclareEventsExchange(typeName, cancellationToken)
            .ConfigureAwait(false);

        var declaredQueue = await channel
            .QueueDeclareAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Unique queue name created by RabbitMQ
        var declaredQueueName = declaredQueue.QueueName;
        await channel
            .QueueBindAsync(declaredQueueName, exchange.ExchangeName,
            topic, null, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return new QueueExchangeBinding(channel, declaredQueueName, exchange.ExchangeName);
    }

    internal static async Task<ExchangeBinding> DeclareEventsExchange(this IChannel channel,
        string exchangeName,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic,
            false, true, null,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return new(channel, exchangeName);
    }

    private static byte[] CreateMessageBody(string json)
        => Encoding.UTF8.GetBytes(json);
}
