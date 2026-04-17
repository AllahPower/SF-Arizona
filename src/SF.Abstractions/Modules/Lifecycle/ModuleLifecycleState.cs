namespace SFSharp;

/// <summary>
/// Current state of a module instance as tracked by <see cref="SFModuleContainer"/>.
/// Exposed through <see cref="ModuleRuntimeSnapshot.State"/>.
/// </summary>
public enum ModuleLifecycleState
{
    /// <summary>Registration exists, but no instance has started yet.</summary>
    Created,

    /// <summary>Container is in <see cref="SFModuleBase.OnStartingAsync"/> or just called the factory.</summary>
    Starting,

    /// <summary><see cref="SFModuleBase.ExecuteAsync(System.Threading.CancellationToken)"/> is in flight.</summary>
    Running,

    /// <summary>A stop has been requested but the task has not completed yet.</summary>
    Stopping,

    /// <summary>Module finished cleanly, either by returning or by honouring cancellation.</summary>
    Stopped,

    /// <summary>Module threw an unhandled exception. See <see cref="ModuleRuntimeSnapshot.LastExceptionType"/>.</summary>
    Faulted
}
