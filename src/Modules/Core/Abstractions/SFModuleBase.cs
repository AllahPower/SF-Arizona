using Microsoft.Extensions.Logging;

namespace SFSharp.Runtime.Modules;

/// <summary>
/// Recommended base class for SF modules. Handles <see cref="ISFModule.RunAsync(IModuleContext)"/>
/// boilerplate through the default interface implementation and exposes protected hook methods for
/// derived modules.
/// </summary>
/// <remarks>
/// Do not override <see cref="ISFModule.RunAsync(IModuleContext)"/> directly. Implement
/// <see cref="ExecuteAsync(CancellationToken)"/> for the main loop and override the <c>On*Async</c>
/// hooks if you need to react to lifecycle transitions.
/// </remarks>
public abstract class SFModuleBase : ISFModule
{
    /// <summary>
    /// Per-run context provided by the container. Never null while
    /// <see cref="ExecuteAsync(CancellationToken)"/> or any <c>On*Async</c> hook is executing,
    /// unavailable outside of an active module run.
    /// </summary>
    protected IModuleContext Context => ((ISFModule)this).Context;

    /// <summary>
    /// Logger scoped to this module's <see cref="ModuleDescriptor.Id"/>. Emitted through the
    /// standard host logging pipeline, so messages hit the configured SF log sinks.
    /// </summary>
    protected ILogger Log => ((ISFModule)this).Log;

    ILogger ISFModule.CreateLogger(IModuleContext context) => CreateLogger(context);
    Task ISFModule.OnStartingAsync() => OnStartingAsync();
    Task ISFModule.OnCompletedAsync() => OnCompletedAsync();
    Task ISFModule.OnFaultedAsync(Exception exception) => OnFaultedAsync(exception);
    Task ISFModule.OnStoppingAsync() => OnStoppingAsync();
    Task ISFModule.OnStoppedAsync() => OnStoppedAsync();
    Task ISFModule.ExecuteAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);

    /// <summary>
    /// Builds the logger assigned to <see cref="Log"/>. Override when you want a different
    /// category name or a custom <see cref="ILogger"/> implementation.
    /// </summary>
    /// <param name="context">Current module context, giving access to <see cref="ModuleDescriptor.Id"/>.</param>
    protected virtual ILogger CreateLogger(IModuleContext context)
    {
        return context.Log;
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
    /// <param name="cancellationToken">Cancelled when the container shuts down or when the user stops the module.</param>
    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}
