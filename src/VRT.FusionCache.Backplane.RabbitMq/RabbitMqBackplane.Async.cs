using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;

partial class RabbitMqBackplane
{

    private async ValueTask EnsureConnectionAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await Task.Yield();
        throw new NotImplementedException();

        //if (_connection is not null)
        //    return;

        //await _connectionLock.WaitAsync(token).ConfigureAwait(false);
        //try
        //{
        //    if (_connection is not null)
        //        return;

        //    if (_options.ConnectionMultiplexerFactory is not null)
        //    {
        //        _connection = await _options.ConnectionMultiplexerFactory().ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        _connection = await ConnectionMultiplexer.ConnectAsync(GetConfigurationOptions());
        //    }

        //    if (_connection is not null)
        //    {
        //        _connection.ConnectionRestored += OnReconnect;
        //        var tmp = _connectHandlerAsync;
        //        if (tmp is not null)
        //        {
        //            await tmp(new BackplaneConnectionInfo(false)).ConfigureAwait(false);
        //        }
        //    }
        //}
        //finally
        //{
        //    _connectionLock.Release();
        //}

        //if (_connection is null)
        //    throw new NullReferenceException("A connection to Redis is not available");

        //EnsureSubscriber();
    }

    /// <inheritdoc/>
    public async ValueTask SubscribeAsync(BackplaneSubscriptionOptions subscriptionOptions)
    {
        SetBackplaneOptions(subscriptionOptions);

        await Task.Yield();
        throw new NotImplementedException();

        //_channel = new RedisChannel(_channelName, RedisChannel.PatternMode.Literal);

        //// CONNECTION
        //await EnsureConnectionAsync().ConfigureAwait(false);

        //if (_subscriber is null)
        //    throw new NullReferenceException("The backplane subscriber is null");

        //await _subscriber.SubscribeAsync(_channel, (rc, value) =>
        //{
        //    var message = GetMessageFromRedisValue(value, _logger, _subscriptionOptions);
        //    if (message is null)
        //        return;

        //    _ = Task.Run(async () =>
        //    {
        //        await OnMessageAsync(message).ConfigureAwait(false);
        //    });
        //}).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask UnsubscribeAsync()
    {
        await Task.Yield();

        _ = Task.Run(() =>
        {
            _incomingMessageHandler = null;
            _incomingMessageHandlerAsync = null;
            //_subscriber?.Unsubscribe(_channel);
            _subscriptionOptions = null;

            //Disconnect();
        });
    }

    /// <inheritdoc/>
    public async ValueTask PublishAsync(BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token = default)
    {
        // CONNECTION
        await EnsureConnectionAsync(token).ConfigureAwait(false);

        //var value = GetRedisValueFromMessage(message, _logger, _subscriptionOptions);

        //if (value.IsNull)
        //    return;

        token.ThrowIfCancellationRequested();

        //await _subscriber!.PublishAsync(_channel, value).ConfigureAwait(false);
    }

    //private async ValueTask OnReconnectAsync(object? sender, ConnectionFailedEventArgs e)
    //{
    //    if (e.ConnectionType == ConnectionType.Subscription)
    //    {
    //        EnsureSubscriber();

    //        _connectHandler?.Invoke(new BackplaneConnectionInfo(true));
    //    }
    //}
}
