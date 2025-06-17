using System.Text;
using System.Text.Json;

namespace VRT.FusionCache.Backplane.RabbitMq.BusService;
internal static class RabbitMqChannelExtensions
{
    private static readonly BasicProperties EventBasicProperties = new()
    {
        DeliveryMode = DeliveryModes.Transient,
        Persistent = false
    };

    public sealed record QueueExchangeBinding(IChannel Channel, string QueueName, string ExchangeName);

    public static Task PublishEvent<T>(this QueueExchangeBinding binding,
        T message,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        return binding.PublishEvent(json, cancellationToken);
    }

    public static async Task PublishEvent(this QueueExchangeBinding binding,
        string message,
        CancellationToken cancellationToken = default)
    {

        var body = CreateMessageBody(message);
        var exchangeName = await binding.Channel.DeclareEventsExchange(binding.ExchangeName, cancellationToken);

        await binding.Channel.BasicPublishAsync(
            binding.ExchangeName, "", false,
            basicProperties: EventBasicProperties, body: body,
            cancellationToken: cancellationToken).AsTask();
    }

    public static async Task<QueueExchangeBinding> ConnectToEvents(this IChannel channel,
        string typeName,

        CancellationToken cancellationToken = default)
    {
        var declaredExchangeName = await channel
            .DeclareEventsExchange(typeName, cancellationToken)
            .ConfigureAwait(false);

        var declaredQueue = await channel
            .QueueDeclareAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Unique queue name created by RabbitMQ
        var declaredQueueName = declaredQueue.QueueName;
        await channel
            .QueueBindAsync(declaredQueueName, declaredExchangeName,
            "", null, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return new QueueExchangeBinding(channel, declaredQueueName, declaredExchangeName);
    }

    internal static async Task<string> DeclareEventsExchange(this IChannel channel,
        string exchangeName,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout,
            false, true, null,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return exchangeName;
    }

    private static byte[] CreateMessageBody(string json)
        => Encoding.UTF8.GetBytes(json);
}
