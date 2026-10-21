namespace SFSharp.Abstractions.Gta;

/// <summary>Frames processed by GTA Idle, not message-loop iterations or rendering callbacks.</summary>
public interface ISFGameClock
{
    bool IsAvailable { get; }
    SFGameTick Current { get; }
    /// <summary>Runs after Process on the game thread, including paused frames. Dispose when the module stops.</summary>
    IDisposable Subscribe(Action<SFGameTick> handler);
}

/// <param name="EngineTimeStep">Native CTimer step, in 1/50 second units; not a wall-clock duration.</param>
public readonly record struct SFGameTick(uint Frame, uint GameMilliseconds, TimeSpan Delta, float EngineTimeStep, bool IsPaused);
