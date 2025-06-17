namespace VRT.FusionCache.Backplane.RabbitMq.Extensions;
internal static class SemaphoreSlimExtensions
{
    public static async Task LockedAsync(this SemaphoreSlim semaphore,
        Func<Task> action,
        Func<bool>? shouldExecuteAction = null,
        CancellationToken cancellationToken = default)
    {
        if (shouldExecuteAction?.Invoke() == false)
        {
            return;
        }
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        // double check to ensure the action should still be executed after acquiring the semaphore
        if (shouldExecuteAction?.Invoke() == false)
        {
            return;
        }
        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static void Locked(this SemaphoreSlim semaphore,
        Action action,
        Func<bool>? shouldExecuteAction = null)
    {
        if (shouldExecuteAction?.Invoke() == false)
        {
            return;
        }
        semaphore.Wait();

        // double check to ensure the action should still be executed after acquiring the semaphore
        if (shouldExecuteAction?.Invoke() == false)
        {
            return;
        }
        try
        {
            action();
        }
        finally
        {
            semaphore.Release();
        }
    }
}
