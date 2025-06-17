using VRT.FusionCache.Backplane.RabbitMq.Extensions;

namespace VRT.FusionCache.Backplane.RabbitMq.BusService.Abstractions;
internal abstract class BaseRabbitMqClient(ConnectionFactory factory) : IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    private SemaphoreSlim _connectionLock = new(initialCount: 1, maxCount: 1);

    protected IChannel? Channel { get; private set; }
    protected IConnection? Connection { get; private set; }
    protected ConnectionFactory Factory { get; } = factory;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue is false)
        {
            if (disposing)
            {
                // add sync dispos
            }
            _disposedValue = true;
        }
    }
    public bool IsConnected()
    {
        return Channel is not null && Channel.IsOpen
            && Connection is not null && Connection.IsOpen;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual Task<IConnection> CreateConnection(CancellationToken cancellationToken = default)
        => Factory.CreateConnectionAsync(cancellationToken);

    protected Task<IChannel> GetChannel(CancellationToken cancellationToken)
    {
        return IsConnected()
            ? Task.FromResult(Channel!)
            : Connect(cancellationToken);
    }

    private async Task<IChannel> Connect(CancellationToken cancellationToken)
    {
        await Disconnect();
        await _connectionLock.LockedAsync(async () =>
        {
            Connection = await CreateConnection(cancellationToken);
            Channel = await Connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }, cancellationToken: cancellationToken);
        return Channel!;
    }

    private Task Disconnect()
    {
        return _connectionLock.LockedAsync(async () =>
        {
            if (Channel is not null)
            {
                await Channel.CloseAsync().ConfigureAwait(false);
                await Channel.DisposeAsync().ConfigureAwait(false);
                Channel = null;
            }
            if (Connection is not null)
            {
                Connection.Dispose();
                Connection = null;
            }
        });
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await Disconnect().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}