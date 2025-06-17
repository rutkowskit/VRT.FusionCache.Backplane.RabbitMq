using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;
internal abstract class BaseRabbitMqSubscriber<T> : BaseRabbitMqClient, IRabbitMqSubscriber<T>
{
    private static readonly JsonSerializerOptions JsonDeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly IMessageHandler<T> _handler;
    private AsyncEventingBasicConsumer? _consumer;
    private Action<IDisposable> _onDispose = (_) => { };

    protected BaseRabbitMqSubscriber(
        ConnectionFactory factory,
        IMessageHandler<T> handler) : base(factory)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }
    public string MessageTypeName { get; private set; } = typeof(T).FullName!;
    protected ILogger? Logger { get; private set; }
    public void WithMessageTypeName(string? messageTypeName)
    {
        if (string.IsNullOrWhiteSpace(messageTypeName))
        {
            return;
        }
        MessageTypeName = messageTypeName;
    }
    public void WithLogger(ILogger logger) => Logger = logger;

    public void SetOnDispose(Action<IDisposable> onDispose)
    {
        ArgumentNullException.ThrowIfNull(onDispose);
        _onDispose = onDispose;
    }
    public async Task Subscribe(CancellationToken cancellationToken = default)
    {
        if (_consumer is not null)
        {
            return;
        }
        var channel = await GetSubscriberChannel(cancellationToken);
        var context = await ConnectToQueue(channel, cancellationToken);
        _consumer = await AttachConsumer(context, cancellationToken);
    }

    public Task Unsubscribe(CancellationToken cancellationToke = default)
    {
        if (_consumer is not null)
        {
            _consumer.ReceivedAsync -= OnMessageReceived;
            _consumer = null;
        }
        return Task.CompletedTask;
    }
    protected abstract Task<ConnectedChannelContext> ConnectToQueue(
        ChannelContext channel,
        CancellationToken cancellation = default);

    private async Task<ChannelContext> GetSubscriberChannel(
        CancellationToken cancellationToken = default)
    {
        var channel = await GetChannel(cancellationToken);
        var queue = channel.BasicQosAsync(0, 1,
            false, cancellationToken: cancellationToken);
        return new ChannelContext(channel!);
    }
    private async Task<AsyncEventingBasicConsumer> AttachConsumer(ConnectedChannelContext context,
        CancellationToken cancellationToken)
    {
        var channel = context.ContextChannel.Channel;
        var queueConsumer = new AsyncEventingBasicConsumer(channel);
        queueConsumer.ReceivedAsync += OnMessageReceived;
        _ = await channel
            .BasicConsumeAsync(context.QueueName, false, queueConsumer, cancellationToken)
            .ConfigureAwait(false);
        return queueConsumer;
    }
    protected virtual async Task OnMessageReceived(object sender, BasicDeliverEventArgs e)
    {
        var channel = await GetChannel(e.CancellationToken);
        if (channel is null || channel.IsOpen is false)
        {
            Logger?.LogWarning("Channel is not open. Cannot process message. {@MessageTypeName}", MessageTypeName);
            return;
        }
        try
        {
            var message = DeserializeBody(e.Body.ToArray());
            await HandleMessage(message!, e.CancellationToken);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "OnMessageReceived failed. {@MessageTypeName} {Error}", MessageTypeName, ex.Message);
            await channel.BasicRejectAsync(e.DeliveryTag, false, e.CancellationToken);
        }
    }

    private async Task HandleMessage(T message, CancellationToken cancellationToken)
    {
        if (message is null)
        {
            Logger?.LogWarning("Received null message of type {MessageTypeName}", MessageTypeName);
            return;
        }
        try
        {
            await _handler.HandleMessageAsync(message!, cancellationToken);
            Logger?.LogInformation("Message handled successfully. {@MessageTypeName}", message.GetType().FullName);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "HandleMessage failed. {@Message} {Error}", message, ex.Message);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await Unsubscribe().ConfigureAwait(false);
        await base.DisposeAsyncCore().ConfigureAwait(false);
        _onDispose(this);
    }

    protected sealed class ConnectedChannelContext(ChannelContext contextChannel)
    {
        public ChannelContext ContextChannel { get; } = contextChannel;
        required public string QueueName { get; init; }
        required public string ExchangeName { get; init; }
    }

    protected sealed class ChannelContext(IChannel channel)
    {
        public IChannel Channel { get; } = channel;
    }

    private T? DeserializeBody(byte[] body)
    {
        try
        {
            var jsonString = Encoding.UTF8.GetString(body) ?? "";
            return JsonSerializer.Deserialize<T>(jsonString, JsonDeserializeOptions);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to deserialize raw message body.");
            return default;
        }
    }

    protected static Task DelayNoThrow(int millisecondsDelay, CancellationToken cancellationToken)
    {
        return Task.Delay(millisecondsDelay, cancellationToken)
            .ContinueWith(_ => { }, cancellationToken);
    }
}