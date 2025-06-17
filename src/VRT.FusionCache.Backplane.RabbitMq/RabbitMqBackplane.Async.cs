using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;

partial class RabbitMqBackplane
{
    private async ValueTask EnsureConnectionAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (_subscriber is not null)
        {
            return;
        }
        await EnsureSubscriber();
        var tmp = _connectHandlerAsync;
        if (tmp is not null)
        {
            await tmp(new BackplaneConnectionInfo(false)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async ValueTask SubscribeAsync(BackplaneSubscriptionOptions subscriptionOptions)
    {
        SetBackplaneOptions(subscriptionOptions);
        await EnsureConnectionAsync().ConfigureAwait(false);

        if (_subscriber is null)
        {
            throw new NullReferenceException("The backplane subscriber is null");
        }
    }

    /// <inheritdoc/>
    public ValueTask UnsubscribeAsync()
    {
        Unsubscribe();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask PublishAsync(BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await EnsureConnectionAsync(token).ConfigureAwait(false);
        await _busService.Publish(message, token).ConfigureAwait(false);
    }
}
