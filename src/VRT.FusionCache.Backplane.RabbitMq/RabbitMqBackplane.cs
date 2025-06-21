using Microsoft.Extensions.Options;
using VRT.FusionCache.Backplane.RabbitMq.Extensions;

namespace VRT.FusionCache.Backplane.RabbitMq;

internal sealed partial class RabbitMqBackplane : IFusionCacheBackplane, IMessageHandler<BackplaneMessage>
{
    private readonly RabbitMqBackplaneOptions _options;
    private readonly SemaphoreSlim _subscriptionSemaphore = new(1, 1);
    private BackplaneSubscriptionOptions? _subscriptionOptions;
    private string? _channelName;
    private Action<BackplaneMessage>? _incomingMessageHandler;
    private Func<BackplaneMessage, ValueTask>? _incomingMessageHandlerAsync;
    private Action<BackplaneConnectionInfo>? _connectHandler;
    private Func<BackplaneConnectionInfo, ValueTask>? _connectHandlerAsync;

    private readonly IBusService _busService;
    private IDisposable? _subscriber;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the RabbitMqBackplane class.
    /// </summary>
    /// <param name="optionsAccessor">The set of options to use with this instance of the backplane.</param>
    /// <param name="busService">The set of options to use with this instance of the backplane.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance to use. If null, logging will be completely disabled.</param>
    public RabbitMqBackplane(
        IOptions<RabbitMqBackplaneOptions> optionsAccessor,
        IBusService busService,
        ILogger<RabbitMqBackplane>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(busService, nameof(busService));
        ArgumentNullException.ThrowIfNull(optionsAccessor, nameof(optionsAccessor));
        ArgumentNullException.ThrowIfNull(optionsAccessor.Value, nameof(optionsAccessor.Value));

        _options = optionsAccessor.Value;
        _logger = logger;
        _busService = busService;
    }

    async Task IMessageHandler<BackplaneMessage>.HandleMessageAsync(BackplaneMessage message, CancellationToken cancellationToken)
    {
        var tmp = _incomingMessageHandlerAsync;
        if (tmp is null)
        {
            if (_logger?.IsEnabled(LogLevel.Trace) ?? false)
            {
                _logger.Log(LogLevel.Trace,
                    "FUSION RabbitMQ [N={CacheName} I={CacheInstanceId}]: [BP] incoming message handler was null",
                    _subscriptionOptions?.CacheName, _subscriptionOptions?.CacheInstanceId);
            }
            return;
        }
        await tmp(message).ConfigureAwait(false);
    }

    private async Task EnsureSubscriber()
    {
        await _subscriptionSemaphore.LockedAsync(
            async () => _subscriber = await _busService
                .SubscribeEvent(this, _channelName ?? "#")
                .ConfigureAwait(false),
            () => _subscriber is null,
            CancellationToken.None).ConfigureAwait(false);
    }

    private void Disconnect()
    {
        _connectHandler = null;
        _connectHandlerAsync = null;

        try
        {
            _subscriptionSemaphore.Locked(
                () => _subscriber?.Dispose(),
                () => _subscriber is not null);
        }
        catch (Exception exc)
        {
            if (_logger?.IsEnabled(LogLevel.Error) ?? false)
            {
                _logger.Log(LogLevel.Error, exc,
                    "FUSION RabbitMq [N={CacheName} I={CacheInstanceId}]: [BP] An error occurred while disconnecting from RabbitMq",
                    _subscriptionOptions?.CacheName, _subscriptionOptions?.CacheInstanceId);
            }
        }
        _subscriber = null;
    }

    private void SetBackplaneOptions(BackplaneSubscriptionOptions subscriptionOptions)
    {
        Validate(subscriptionOptions);
        _subscriptionOptions = subscriptionOptions;
        _channelName = _subscriptionOptions.ChannelName;
        _incomingMessageHandler = _subscriptionOptions.IncomingMessageHandler;
        _connectHandler = _subscriptionOptions.ConnectHandler;
        _incomingMessageHandlerAsync = _subscriptionOptions.IncomingMessageHandlerAsync;
        _connectHandlerAsync = _subscriptionOptions.ConnectHandlerAsync;
    }

    private void Validate(BackplaneSubscriptionOptions subscriptionOptions)
    {
        if (subscriptionOptions is null)
            throw new ArgumentNullException(nameof(subscriptionOptions));

        if (subscriptionOptions.ChannelName is null)
            throw new NullReferenceException("The BackplaneSubscriptionOptions.ChannelName cannot be null");

        if (subscriptionOptions.IncomingMessageHandler is null)
            throw new NullReferenceException("The BackplaneSubscriptionOptions.IncomingMessageHandler cannot be null");

        if (subscriptionOptions.ConnectHandler is null)
            throw new NullReferenceException("The BackplaneSubscriptionOptions.ConnectHandler cannot be null");

        if (subscriptionOptions.IncomingMessageHandlerAsync is null)
            throw new NullReferenceException("The BackplaneSubscriptionOptions.IncomingMessageHandlerAsync cannot be null");

        if (subscriptionOptions.ConnectHandlerAsync is null)
            throw new NullReferenceException("The BackplaneSubscriptionOptions.ConnectHandlerAsync cannot be null");
    }
}
