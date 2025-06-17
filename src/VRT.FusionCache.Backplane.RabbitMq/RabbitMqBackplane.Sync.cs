using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;

partial class RabbitMqBackplane
{
    /// <inheritdoc/>
	public void Subscribe(BackplaneSubscriptionOptions subscriptionOptions)
    {
        SetBackplaneOptions(subscriptionOptions);
        EnsureSubscriber().ConfigureAwait(false).GetAwaiter().GetResult();
        if (_subscriber is null)
        {
            throw new NullReferenceException("The backplane subscriber is null");
        }
    }

    /// <inheritdoc/>
    public void Unsubscribe()
    {
        _incomingMessageHandler = null;
        _incomingMessageHandlerAsync = null;
        _subscriptionOptions = null;
        Disconnect();
    }

    /// <inheritdoc/>
    public void Publish(BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token = default)
    {
        _ = Task.Run(() => _ = PublishAsync(message, options, token), token);
    }
}
