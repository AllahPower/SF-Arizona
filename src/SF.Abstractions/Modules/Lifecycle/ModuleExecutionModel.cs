namespace SFSharp.Abstractions.Modules.Lifecycle;

/// <summary>
/// Controls which thread a module runs on when the container starts it.
/// Configured via <see cref="SFModuleAttribute.ExecutionModel"/>.
/// </summary>
public enum ModuleExecutionModel
{
    /// <summary>
    /// Module runs inline on the game main thread. Safe for direct calls into SAMP internals
    /// and memory patches. Long synchronous work will stall the game loop, so use awaits.
    /// </summary>
    MainThread,

    /// <summary>
    /// Module runs on a worker thread via <see cref="System.Threading.Tasks.Task.Run(System.Action)"/>.
    /// Anything touching game state must execute through <see cref="ISF.MainThread"/>.
    /// </summary>
    BackgroundWorker,

    /// <summary>
    /// Module starts on the main thread but is free to switch to background work through
    /// <see cref="IModuleContext.RunBackground(System.Func{System.Threading.CancellationToken, System.Threading.Tasks.Task})"/> and back through
    /// <see cref="ISF.MainThread"/>.
    /// </summary>
    Hybrid
}
