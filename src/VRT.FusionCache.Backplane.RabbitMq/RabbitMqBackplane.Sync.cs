using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;

partial class RabbitMqBackplane
{


    /// <inheritdoc/>
	public void Subscribe(BackplaneSubscriptionOptions subscriptionOptions)
    {
        Validate(subscriptionOptions);

        //_channel = new RedisChannel(_channelName, RedisChannel.PatternMode.Literal);



        throw new NotImplementedException();

        //// CONNECTION
        //EnsureConnection();

        //if (_subscriber is null)
        //    throw new NullReferenceException("The backplane subscriber is null");

        //_subscriber.Subscribe(_channel, (rc, value) =>
        //{
        //    var message = GetMessageFromRedisValue(value, _logger, _subscriptionOptions);
        //    if (message is null)
        //        return;

        //    _ = Task.Run(async () =>
        //    {
        //        await OnMessageAsync(message).ConfigureAwait(false);
        //    });
        //});
    }

    /// <inheritdoc/>
    public void Unsubscribe()
    {
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
    public void Publish(BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token = default)
    {
        throw new NotImplementedException();

        // CONNECTION
        //EnsureConnection(token);

        //var value = GetRedisValueFromMessage(message, _logger, _subscriptionOptions);

        //if (value.IsNull)
        //    return;

        //token.ThrowIfCancellationRequested();

        //_subscriber!.Publish(_channel, value);
    }
}
