namespace SFSharp.Runtime.Interop.Hooking;

internal enum SampObservationAbi : byte { Void, Pointer, TimedSync, Int, Id, Entry, Pickup }

/// <summary>Host-owned group. An installation failure disables prior entries and is cached, not retried over partial patches.</summary>
internal sealed class SampObservationHooks : IDisposable
{
    private readonly List<IDisposable> _hooks = [];
    public void Own(IDisposable resource) => _hooks.Add(resource);
    public static SampObservationHooks Create(Action<SampObservationHooks> install)
    {
        var hooks = new SampObservationHooks();
        try { install(hooks); return hooks; }
        catch { hooks.Dispose(); throw; }
    }

    public void Add(SampObservationPoint point, SampObservationAbi abi, ISubHook<SampObservationArgs, NoRetValue> observer)
    {
        int rva = SampObservationOffsets.Resolve(SampEnvironment.RequiredProfile.Version, point);
        nint address = ModuleResolver.GetProcAddress("samp.dll", rva);
        IDisposable hook = abi switch
        {
            SampObservationAbi.Void => new SampObserveVoidHook(address, observer),
            SampObservationAbi.Pointer => new SampObservePointerHook(address, observer),
            SampObservationAbi.TimedSync => new SampObserveTimedSyncHook(address, observer),
            SampObservationAbi.Int => new SampObserveIntHook(address, observer),
            SampObservationAbi.Id => new SampObserveIdHook(address, observer),
            SampObservationAbi.Entry => new SampObserveEntryHook(address, observer),
            SampObservationAbi.Pickup => new SampObservePickupHook(address, observer),
            _ => throw new ArgumentOutOfRangeException(nameof(abi)),
        };
        _hooks.Add(hook);
    }

    public void Dispose()
    {
        for (int i = _hooks.Count - 1; i >= 0; i--)
        {
            try { _hooks[i].Dispose(); }
            catch (Exception ex) { HookRuntime.ReportSafely(ex); }
        }
    }
}
