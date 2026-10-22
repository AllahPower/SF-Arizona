using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

// Raw ABI slots stay inside Interop. Subscribers receive copied domain events, never these pointers.
internal readonly record struct SampObservationArgs(nint Instance, nint Data = 0, int Value = 0, int Extra = 0, uint Timestamp = 0);

internal sealed class SampObservation<T>(Func<SampObservationArgs, T> capture, Action<SampObservationArgs, T> complete, Func<bool>? active = null)
    : ISubHook<SampObservationArgs, NoRetValue>
{
    public NoRetValue Process(SampObservationArgs args, Func<SampObservationArgs, NoRetValue> next)
    {
        if (active is not null && !active()) return next(args);
        T snapshot = capture(args);
        NoRetValue result = next(args);
        complete(args, snapshot);
        return result;
    }
}

internal sealed class SampObserveVoidHook : NativeHook<SampObservationArgs, NoRetValue, SampObserveVoidHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance);
    public SampObserveVoidHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance) => ProcessNative(new(instance), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance); return default; }
}

internal sealed class SampObservePointerHook : NativeHook<SampObservationArgs, NoRetValue, SampObservePointerHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance, nint data);
    public SampObservePointerHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance, nint data) => ProcessNative(new(instance, Data: data), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance, args.Data); return default; }
}

internal sealed class SampObserveTimedSyncHook : NativeHook<SampObservationArgs, NoRetValue, SampObserveTimedSyncHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance, nint data, uint timestamp);
    public SampObserveTimedSyncHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance, nint data, uint timestamp) => ProcessNative(new(instance, Data: data, Timestamp: timestamp), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance, args.Data, args.Timestamp); return default; }
}

internal sealed class SampObserveIntHook : NativeHook<SampObservationArgs, NoRetValue, SampObserveIntHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance, int value);
    public SampObserveIntHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance, int value) => ProcessNative(new(instance, Value: value), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance, args.Value); return default; }
}

internal sealed class SampObserveIdHook : NativeHook<SampObservationArgs, NoRetValue, SampObserveIdHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance, ushort id);
    public SampObserveIdHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance, ushort id) => ProcessNative(new(instance, Value: id), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance, (ushort)args.Value); return default; }
}

internal sealed class SampObserveEntryHook : NativeHook<SampObservationArgs, NoRetValue, SampObserveEntryHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance, ushort id, int passenger);
    public SampObserveEntryHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance, ushort id, int passenger) => ProcessNative(new(instance, Value: id, Extra: passenger), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance, (ushort)args.Value, args.Extra); return default; }
}

internal sealed class SampObservePickupHook : NativeHook<SampObservationArgs, NoRetValue, SampObservePickupHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)] internal delegate void Native(nint instance, nint data, int id);
    public SampObservePickupHook(nint address, ISubHook<SampObservationArgs, NoRetValue> observer)
    { AddSubHook(observer); InstallHook(address, new Native(Call)); }
    private void Call(nint instance, nint data, int id) => ProcessNative(new(instance, Data: data, Value: id), default);
    protected override NoRetValue InvokeOriginalFunction(SampObservationArgs args)
    { OriginalFunction(args.Instance, args.Data, args.Value); return default; }
}
