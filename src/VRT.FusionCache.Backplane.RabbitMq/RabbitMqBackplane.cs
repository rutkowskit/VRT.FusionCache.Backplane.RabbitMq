using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VRT.FusionCache.Backplane.RabbitMq;

public sealed partial class RabbitMqBackplane : IFusionCacheBackplane
{
    private readonly RabbitMqBackplaneOptions _options;
    private readonly SemaphoreSlim _connectionLock;

    private BackplaneSubscriptionOptions? _subscriptionOptions;
    private Action<BackplaneMessage>? _incomingMessageHandler;
    private Func<BackplaneMessage, ValueTask>? _incomingMessageHandlerAsync;
    private Action<BackplaneConnectionInfo>? _connectHandler;
    private Func<BackplaneConnectionInfo, ValueTask>? _connectHandlerAsync;
    private string? _channelName;

    //private readonly ILogger? _logger;

    //private Action<BackplaneConnectionInfo>? _connectHandler;
    //private Func<BackplaneConnectionInfo, ValueTask>? _connectHandlerAsync;


    /// <summary>
    /// Initializes a new instance of the RabbitMqBackplane class.
    /// </summary>
    /// <param name="optionsAccessor">The set of options to use with this instance of the backplane.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance to use. If null, logging will be completely disabled.</param>
    public RabbitMqBackplane(IOptions<RabbitMqBackplaneOptions> optionsAccessor, ILogger<RabbitMqBackplane>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor, nameof(optionsAccessor));
        ArgumentNullException.ThrowIfNull(optionsAccessor.Value, nameof(optionsAccessor.Value));

        _options = optionsAccessor.Value;
        _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
    }

    private RabbitMqBackplaneOptions.RabbitMqOptions GetRabbitMqConfigurationOptions()
    {
        if (_options.RabbitMq is not null)
            return _options.RabbitMq;

        //if (string.IsNullOrWhiteSpace(_options.Configuration) == false)
        //    return ConfigurationOptions.Parse(_options.Configuration!);

        throw new InvalidOperationException("Unable to connect to Redis: no Configuration nor ConfigurationOptions have been specified");
    }

    //private void EnsureSubscriber()
    //{
    //    if (_subscriber is null && _connection is not null)
    //        _subscriber = _connection.GetSubscriber();
    //}

    //private void Disconnect()
    //{
    //    _connectHandler = null;
    //    _connectHandlerAsync = null;

    //    if (_connection is null)
    //        return;

    //    try
    //    {
    //        _connection.ConnectionRestored -= OnReconnect;
    //        _connection.Dispose();
    //    }
    //    catch (Exception exc)
    //    {
    //        if (_logger?.IsEnabled(LogLevel.Error) ?? false)
    //            _logger.Log(LogLevel.Error, exc, "FUSION [N={CacheName} I={CacheInstanceId}]: [BP] An error occurred while disconnecting from Redis {Config}", _subscriptionOptions?.CacheName, _subscriptionOptions?.CacheInstanceId, _options.ConfigurationOptions?.ToString() ?? _options.Configuration);
    //    }

    //    _connection = null;
    //}

    //private static BackplaneMessage? GetMessageFromRedisValue(RedisValue value, ILogger? logger, BackplaneSubscriptionOptions? subscriptionOptions)
    //{
    //    try
    //    {
    //        return BackplaneMessage.FromByteArray(value);
    //    }
    //    catch (Exception exc)
    //    {
    //        if (logger?.IsEnabled(LogLevel.Warning) ?? false)
    //            logger.Log(LogLevel.Warning, exc, "FUSION [N={CacheName} I={CacheInstanceId}]: [BP] an error occurred while converting a RedisValue into a BackplaneMessage", subscriptionOptions?.CacheName, subscriptionOptions?.CacheInstanceId);
    //    }

    //    return null;
    //}

    //private static RedisValue GetRedisValueFromMessage(BackplaneMessage message, ILogger? logger, BackplaneSubscriptionOptions? subscriptionOptions)
    //{
    //    try
    //    {
    //        return BackplaneMessage.ToByteArray(message);
    //    }
    //    catch (Exception exc)
    //    {
    //        if (logger?.IsEnabled(LogLevel.Warning) ?? false)
    //            logger.Log(LogLevel.Warning, exc, "FUSION [N={CacheName} I={CacheInstanceId}]: [BP] an error occurred while converting a BackplaneMessage into a RedisValue", subscriptionOptions?.CacheName, subscriptionOptions?.CacheInstanceId);
    //    }

    //    return RedisValue.Null;
    //}

    //internal async ValueTask OnMessageAsync(BackplaneMessage message)
    //{
    //    var tmp = _incomingMessageHandlerAsync;
    //    if (tmp is null)
    //    {
    //        if (_logger?.IsEnabled(LogLevel.Trace) ?? false)
    //            _logger.Log(LogLevel.Trace, "FUSION [N={CacheName} I={CacheInstanceId}]: [BP] incoming message handler was null", _subscriptionOptions?.CacheName, _subscriptionOptions?.CacheInstanceId);
    //        return;
    //    }

    //    await tmp(message).ConfigureAwait(false);
    //}

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
