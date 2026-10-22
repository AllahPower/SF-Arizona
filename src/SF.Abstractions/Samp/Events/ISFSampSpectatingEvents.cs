namespace SFSharp.Abstractions.Samp.Events;

/// <summary>Reports actual enabled/target state after native spectating routines, not requested values.</summary>
public interface ISFSampSpectatingEvents
{
    IDisposable OnChanged(Action<SFSampSpectatingEvent> handler);
}
