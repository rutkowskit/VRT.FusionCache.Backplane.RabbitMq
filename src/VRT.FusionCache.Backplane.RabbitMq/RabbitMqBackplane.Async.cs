using ZiggyCreatures.Caching.Fusion;

namespace VRT.FusionCache.Backplane.RabbitMq;

partial class RabbitMqBackplane
{
    private static readonly ValueTask CompletedValueTask = new(Task.CompletedTask);
    private async ValueTask EnsureConnectionAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (_subscriber is not null)
        {
            return;
        }
        await EnsureSubscriber(token);
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
        return CompletedValueTask;
    }

    /// <inheritdoc/>
    public async ValueTask PublishAsync(BackplaneMessage message, FusionCacheEntryOptions options, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await EnsureConnectionAsync(token).ConfigureAwait(false);
        await _busService.Publish(message, _channelName ?? "", token).ConfigureAwait(false);
    }
}
