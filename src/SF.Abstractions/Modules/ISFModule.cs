using Microsoft.Extensions.Logging;

namespace SFSharp.Abstractions.Modules;

/// <summary>
/// Minimal contract implemented by every SF module. User modules typically derive from
/// <c>SFModuleBase</c> instead of implementing this directly, but the interface now also exposes
/// the same lifecycle-oriented contract via default interface implementations.
/// </summary>
/// <remarks>
/// Registered types are resolved through a parameterless constructor. Keep state inside the instance,
/// the host creates a fresh instance for every restart.
/// </remarks>
public interface ISFModule
{
    /// <summary>
    /// Per-run context provided by the host. Available only while <see cref="RunAsync(IModuleContext)"/>,
    /// <see cref="ExecuteAsync(CancellationToken)"/> or lifecycle hooks are executing.
    /// </summary>
    IModuleContext Context => ModuleExecutionStateStore.GetContext(this)
        ?? throw new InvalidOperationException("Module context is available only during an active module run.");

    /// <summary>
    /// Logger for the current module run. Available only while <see cref="RunAsync(IModuleContext)"/>,
    /// <see cref="ExecuteAsync(CancellationToken)"/> or lifecycle hooks are executing.
    /// </summary>
    ILogger Log => ModuleExecutionStateStore.GetLog(this)
        ?? throw new InvalidOperationException("Module logger is available only during an active module run.");

    /// <summary>
    /// Entry point invoked by the host after the module is constructed. The returned task represents
    /// the whole lifetime of the module, it it completes either when the module finishes naturally or
    /// when <paramref name="context"/>'s cancellation token fires.
    /// </summary>
    /// <param name="context">
    /// Per-run context with telemetry, storage and registration helpers. Valid only until the returned
    /// task completes, the host disposes it afterwards.
    /// </param>
    async Task RunAsync(IModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ILogger log = CreateLogger(context);
        ModuleExecutionStateStore.Attach(this, context, log);

        await OnStartingAsync();
        try
        {
            await ExecuteAsync(context.CancellationToken);
            await OnCompletedAsync();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await OnFaultedAsync(ex);
            throw;
        }
        finally
        {
            try
            {
                await OnStoppingAsync();
            }
            finally
            {
                try
                {
                    await OnStoppedAsync();
                }
                finally
                {
                    ModuleExecutionStateStore.Clear(this);
                }
            }
        }
    }

    /// <summary>
    /// Builds the logger assigned to <see cref="Log"/> for the current run.
    /// </summary>
    /// <param name="context">Current module context.</param>
    ILogger CreateLogger(IModuleContext context) => context.Log;

    /// <summary>Runs after <see cref="Context"/> and <see cref="Log"/> are prepared, before <see cref="ExecuteAsync(CancellationToken)"/>.</summary>
    Task OnStartingAsync() => Task.CompletedTask;

    /// <summary>Runs when <see cref="ExecuteAsync(CancellationToken)"/> completed normally.</summary>
    Task OnCompletedAsync() => Task.CompletedTask;

    /// <summary>Runs when <see cref="ExecuteAsync(CancellationToken)"/> throws a non-cancellation exception.</summary>
    /// <param name="exception">Unhandled exception from the module body.</param>
    Task OnFaultedAsync(Exception exception) => Task.CompletedTask;

    /// <summary>Always runs during teardown. Use for deterministic cleanup.</summary>
    Task OnStoppingAsync() => Task.CompletedTask;

    /// <summary>Final lifecycle hook for the current run.</summary>
    Task OnStoppedAsync() => Task.CompletedTask;

    /// <summary>
    /// Main module body. Implement this to participate in the default lifecycle pipeline without
    /// overriding <see cref="RunAsync(IModuleContext)"/> directly.
    /// </summary>
    /// <param name="cancellationToken">Cancelled when the container shuts down or stops the module.</param>
    Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
