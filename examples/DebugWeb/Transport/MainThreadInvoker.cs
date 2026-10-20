namespace SFSharp.Examples.DebugWeb.Transport;

internal sealed class MainThreadInvoker(SynchronizationContext context)
{
    public async Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken)
    {
        TaskCompletionSource<T> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        context.Post(_ =>
        {
            if (cancellationToken.IsCancellationRequested) return;
            try { completion.TrySetResult(action()); }
            catch (Exception ex) { completion.TrySetException(ex); }
        }, null);
        return await completion.Task.ConfigureAwait(false);
    }
}
