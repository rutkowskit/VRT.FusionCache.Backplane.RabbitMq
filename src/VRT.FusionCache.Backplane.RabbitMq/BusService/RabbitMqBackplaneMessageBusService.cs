using Microsoft.Extensions.Options;
using System.Text.Json;

namespace VRT.FusionCache.Backplane.RabbitMq.BusService;
internal sealed class RabbitMqBackplaneMessageBusService : BaseRabbitMqClient, IBusService
{
    private readonly ConcurrentDictionary<IDisposable, BaseRabbitMqClient> _subscribers;
    private readonly ILogger<RabbitMqBackplaneMessageBusService> _logger;
    private readonly RabbitMqInstance _instance;
    private readonly RabbitMqBackplaneOptions _options;

    public RabbitMqBackplaneMessageBusService(
        ConnectionFactory connectionFactory,
        ILogger<RabbitMqBackplaneMessageBusService> logger,
        IOptions<RabbitMqBackplaneOptions> options,
        RabbitMqInstance instance)
        : base(connectionFactory)
    {
        _subscribers = [];
        _logger = logger;
        _instance = instance;
        _options = options.Value;
    }
    protected override void Dispose(bool disposing)
    {
        var toRemove = _subscribers.Keys.ToList();
        toRemove.ForEach(s => s.Dispose());
        _subscribers.Clear();
        base.Dispose(disposing);
    }

    public async Task Publish(
        BackplaneMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var channel = await GetChannel(cancellationToken);
            var json = JsonSerializer.Serialize(message);
            var binding = await channel.ConnectToEvents(_options.ExchangeName, cancellationToken);
            await binding.PublishEvent(json, _instance, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while publishing message: {Message}", message);
            throw;
        }
    }

    public async Task<IDisposable> SubscribeEvent(
        IMessageHandler<BackplaneMessage> handler,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriber = new RabbitMqEventSubscriber<BackplaneMessage>(Factory, handler, _instance);
            subscriber.WithMessageTypeName(_options.ExchangeName);
            subscriber.WithLogger(_logger);
            subscriber.SetOnDispose(RemoveDisposedSubscriber);
            _subscribers.TryAdd(subscriber, subscriber);
            await subscriber.Subscribe(cancellationToken);
            return new Disposable(() => RemoveDisposedSubscriber(subscriber));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while subscribing to {ExchangeName}", _options.ExchangeName);
            throw;
        }
    }

    private void RemoveDisposedSubscriber(IDisposable subscriber)
    {
        if (_subscribers.TryRemove(subscriber, out var removed))
        {
            removed.Dispose();
        }
    }

    private sealed record Disposable(Action OnDispose) : IDisposable
    {
        public void Dispose() => OnDispose();
    }
}