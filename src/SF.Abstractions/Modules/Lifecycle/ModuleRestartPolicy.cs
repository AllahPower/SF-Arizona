namespace SFSharp.Abstractions.Modules.Lifecycle;

/// <summary>
/// Controls whether a faulted module is automatically restarted by <see cref="SFModuleContainer"/>.
/// Configured via <see cref="SFModuleAttribute.RestartPolicy"/>.
/// </summary>
public enum ModuleRestartPolicy
{
    /// <summary>
    /// Module stays stopped after a fault. The user can restart it through the <c>/sfs</c> dashboard.
    /// </summary>
    Manual,

    /// <summary>
    /// Container restarts the module after a fault. A circuit breaker trips if faults repeat,
    /// after which the module is disabled and requires manual intervention.
    /// </summary>
    OnFault
}
