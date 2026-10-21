namespace SFSharp.Abstractions.Gta;

/// <summary>Frames processed by GTA Idle, not message-loop iterations or rendering callbacks.</summary>
public interface ISFGameClock
{
    bool IsAvailable { get; }
    SFGameTick Current { get; }
    /// <summary>Runs after Process on the game thread, including paused frames. Dispose when the module stops.</summary>
    IDisposable Subscribe(Action<SFGameTick> handler);
}

/// <param name="Frame">GTA frame counter.</param>
/// <param name="GameMilliseconds">GTA game timer, which can wrap or reset.</param>
/// <param name="Delta">Elapsed game time since the previous tick.</param>
/// <param name="EngineTimeStep">Native CTimer step, in 1/50 second units; not a wall-clock duration.</param>
/// <param name="IsPaused">Whether gameplay is paused.</param>
public readonly record struct SFGameTick(uint Frame, uint GameMilliseconds, TimeSpan Delta, float EngineTimeStep, bool IsPaused);
