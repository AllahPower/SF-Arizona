using System.Diagnostics;

namespace SFSharp.Runtime.Modules;

/// <summary>
/// Concrete <see cref="IModuleTelemetry"/> facade. Thread-safe - delegates to
/// <see cref="ModuleRuntimeInfo"/>, which already guards its own state.
/// </summary>
internal sealed class ModuleTelemetry : IModuleTelemetry
{
    private readonly ModuleRuntimeInfo _runtime;

    internal ModuleTelemetry(ModuleRuntimeInfo runtime)
    {
        _runtime = runtime;
    }

    public void Heartbeat(string? activity = null) => _runtime.RecordHeartbeat(activity);

    public void ReportActivity(string activity) => _runtime.RecordActivity(activity);

    public void SetStatusText(string? value) => _runtime.SetStatusText(value);

    public void IncrementCounter(string counterName, long delta = 1) => _runtime.IncrementCounter(counterName, delta);

    public void SetDetail(string key, string? value) => _runtime.SetDetail(key, value);

    public IDisposable TrackLoop(string? activity = null) =>
        new ModuleLoopScope(_runtime, Stopwatch.GetTimestamp(), activity);

    public ModuleRuntimeSnapshot GetSnapshot() => _runtime.CreateSnapshot();
}
