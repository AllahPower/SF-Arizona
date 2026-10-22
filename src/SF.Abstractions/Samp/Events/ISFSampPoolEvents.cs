namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Session pool reset and initialization boundaries. No native pool pointer is retained.</summary>
public interface ISFSampPoolEvents
{
    /// <summary>Before/after each routine, including nested resets. Use Before to invalidate cached IDs and handles.</summary>
    IDisposable OnChanged(Action<SFSampPoolEvent> handler);
}
