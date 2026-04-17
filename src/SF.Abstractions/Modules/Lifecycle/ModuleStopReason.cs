namespace SFSharp;

/// <summary>
/// Records why a module left the <see cref="ModuleLifecycleState.Running"/> state.
/// Surfaced through <see cref="ModuleRuntimeSnapshot.LastStopReason"/> and drives restart logic in
/// <see cref="SFModuleContainer"/>.
/// </summary>
public enum ModuleStopReason
{
    /// <summary>No stop has been recorded yet for this lifecycle.</summary>
    None,

    /// <summary>Module returned from <see cref="SFModuleBase.ExecuteAsync(System.Threading.CancellationToken)"/> on its own.</summary>
    Completed,

    /// <summary>User invoked <c>/sfs stop</c> or pressed <c>[Stop]</c> in the dashboard.</summary>
    UserRequested,

    /// <summary>User invoked <c>/sfs restart</c> or pressed <c>[Restart]</c>. The container restarts immediately after teardown.</summary>
    RestartRequested,

    /// <summary>Container itself is shutting down and is cancelling every running module.</summary>
    ContainerShutdown,

    /// <summary>Module threw an unhandled exception. If <see cref="SFModuleAttribute.RestartPolicy"/> is
    /// <see cref="ModuleRestartPolicy.OnFault"/> and the circuit breaker has not tripped, a restart follows.</summary>
    Faulted,

    /// <summary>Host is unloading the plugin that owns this module. No restart attempted.</summary>
    PluginUnload
}
