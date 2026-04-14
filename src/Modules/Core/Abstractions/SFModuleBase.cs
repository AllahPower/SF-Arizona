using Microsoft.Extensions.Logging;

namespace SFSharp;

/// <summary>
/// Recommended base class for SF modules. Handles <see cref="ISFModule.RunAsync(ModuleContext)"/>
/// boilerplate: wires <see cref="Context"/> and <see cref="Log"/>, calls the lifecycle hooks in the
/// right order, swallows the cooperative <see cref="OperationCanceledException"/>, and propagates
/// other exceptions to the container so they reach telemetry and the restart policy.
/// </summary>
/// <remarks>
/// Do not override <see cref="ISFModule.RunAsync(ModuleContext)"/> directly. Implement
/// <see cref="ExecuteAsync(CancellationToken)"/> for the main loop and override the <c>On*Async</c>
/// hooks if you need to react to lifecycle transitions.
/// </remarks>
public abstract class SFModuleBase : ISFModule
{
    /// <summary>
    /// Per-run context provided by the container. Never null while
    /// <see cref="ExecuteAsync(CancellationToken)"/> or any <c>On*Async</c> hook is executing,
    /// cleared back to <see langword="null"/> once the module tears down.
    /// </summary>
    protected ModuleContext Context { get; private set; } = null!;

    /// <summary>
    /// Logger scoped to this module's <see cref="ModuleDescriptor.Id"/>. Emitted through the
    /// standard SF logger provider, so messages hit both the in-game chat sink and <c>sf_arz.log</c>.
    /// </summary>
    protected ILogger Log { get; private set; } = null!;

    /// <inheritdoc/>
    /// <remarks>
    /// Sealed for base-class consumers. Override <see cref="ExecuteAsync(CancellationToken)"/>
    /// and the <c>On*Async</c> hooks instead.
    /// </remarks>
    public async Task RunAsync(IModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = (ModuleContext)context;
        Log = CreateLogger(context);

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
                await OnStoppedAsync();
                Context = null!;
                Log = null!;
            }
        }
    }

    /// <summary>
    /// Builds the logger assigned to <see cref="Log"/>. Override when you want a different
    /// category name or a custom <see cref="ILogger"/> implementation.
    /// </summary>
    /// <param name="context">Current module context, giving access to <see cref="ModuleDescriptor.Id"/>.</param>
    protected virtual ILogger CreateLogger(IModuleContext context)
    {
        return SFLoggerProvider.Instance.CreateLogger(context.Descriptor.Id);
    }

    /// <summary>Runs after <see cref="Context"/> and <see cref="Log"/> are set, before <see cref="ExecuteAsync(CancellationToken)"/>.</summary>
    protected virtual Task OnStartingAsync() => Task.CompletedTask;

    /// <summary>Runs when <see cref="ExecuteAsync(CancellationToken)"/> returns normally, before the <c>OnStopping</c>/<c>OnStopped</c> pair.</summary>
    protected virtual Task OnCompletedAsync() => Task.CompletedTask;

    /// <summary>
    /// Runs when <see cref="ExecuteAsync(CancellationToken)"/> threw a non-cancellation exception.
    /// The exception is still rethrown to the container afterwards.
    /// </summary>
    /// <param name="exception">The unhandled exception, before <see cref="Exception.GetBaseException"/>.</param>
    protected virtual Task OnFaultedAsync(Exception exception) => Task.CompletedTask;

    /// <summary>Always runs in the finally block, use it for deterministic cleanup such as flushing state.</summary>
    protected virtual Task OnStoppingAsync() => Task.CompletedTask;

    /// <summary>Last hook before <see cref="Context"/> is cleared. Runs even when <see cref="OnStoppingAsync"/> throws.</summary>
    protected virtual Task OnStoppedAsync() => Task.CompletedTask;

    /// <summary>
    /// Main module loop. Return when the module is done, or await a cancellable operation on
    /// <paramref name="cancellationToken"/> and let it throw <see cref="OperationCanceledException"/>
    /// to stop cleanly.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancelled when the container is shutting down or when the user issued <c>/sfs stop</c>.
    /// Aliased by <see cref="ModuleContext.CancellationToken"/>.
    /// </param>
    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}
