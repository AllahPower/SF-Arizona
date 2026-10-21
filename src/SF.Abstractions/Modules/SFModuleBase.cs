using Microsoft.Extensions.Logging;

namespace SFSharp.Abstractions.Modules;

/// <summary>Optional lifecycle implementation with instance-owned run state.</summary>
public abstract class SFModuleBase : ISFModule
{
    private IModuleContext? _context;
    private ILogger? _log;
    private int _running;

    /// <summary>Current run context; unavailable before startup and after teardown.</summary>
    protected IModuleContext Context => _context
        ?? throw new InvalidOperationException("The module has no active run.");

    /// <summary>Logger for the current run.</summary>
    protected ILogger Log => _log
        ?? throw new InvalidOperationException("The module has no active run.");

    /// <summary>Runs startup, execution and teardown once. Concurrent runs are rejected.</summary>
    public async Task RunAsync(IModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            throw new InvalidOperationException("The module is already running.");

        _context = context;
        try
        {
            _log = CreateLogger(context);
            try
            {
                await OnStartingAsync();
                await ExecuteAsync(context.CancellationToken);
                await OnCompletedAsync();
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await OnFaultedAsync(exception);
                throw;
            }
        }
        finally
        {
            try
            {
                try { await OnStoppingAsync(); }
                finally { await OnStoppedAsync(); }
            }
            finally
            {
                _log = null;
                _context = null;
                Volatile.Write(ref _running, 0);
            }
        }
    }

    protected virtual ILogger CreateLogger(IModuleContext context) => context.Log;
    protected virtual Task OnStartingAsync() => Task.CompletedTask;
    protected virtual Task OnCompletedAsync() => Task.CompletedTask;
    protected virtual Task OnFaultedAsync(Exception exception) => Task.CompletedTask;
    protected virtual Task OnStoppingAsync() => Task.CompletedTask;
    protected virtual Task OnStoppedAsync() => Task.CompletedTask;
    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}
