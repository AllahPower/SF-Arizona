namespace SFSharp.Runtime.Modules.Hosting;

internal sealed class ModuleAsyncCommand(ModuleRunScope scope, Func<string?, CancellationToken, Task> callback)
{
    private int _busy;

    public void Invoke(string? args)
    {
        // Reject overlap before executing the synchronous prefix, including reentrant commands.
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return;
        try
        {
            _ = scope.Run(async token =>
            {
                try { await callback(args, token); }
                finally { Volatile.Write(ref _busy, 0); }
            }, background: false);
        }
        catch
        {
            Volatile.Write(ref _busy, 0);
            throw;
        }
    }
}
