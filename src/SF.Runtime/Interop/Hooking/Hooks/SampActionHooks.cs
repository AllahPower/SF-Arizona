using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

/// <summary>Runs observers only after the trampoline has returned, without changing its result.</summary>
internal sealed class SampActionObserver<TArgs, TResult>(Action<TArgs, TResult> completed) : ISubHook<TArgs, TResult>
{
    public TResult Process(TArgs args, Func<TArgs, TResult> next)
    {
        TResult result = next(args);
        completed(args, result);
        return result;
    }
}

/// <summary>No stack arguments: RequestSpawn, WastedNotification and ShutdownForRestart.</summary>
internal sealed class SampVoidActionHook : NativeHook<nint, NoRetValue, SampVoidActionHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint instance);

    public SampVoidActionHook(nint address, Action completed)
    {
        AddSubHook(new SampActionObserver<nint, NoRetValue>((_, _) => completed()));
        InstallHook(address, new Native(HookProc));
    }

    private void HookProc(nint instance) => ProcessNative(instance, default);

    protected override NoRetValue InvokeOriginalFunction(nint instance)
    {
        OriginalFunction(instance);
        return default;
    }
}

/// <summary>Spawn returns a Win32 BOOL in EAX, not a one-byte C++ bool in AL.</summary>
internal sealed class SampSpawnActionHook : NativeHook<nint, int, SampSpawnActionHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint instance);

    public SampSpawnActionHook(nint address, Action<bool> completed)
    {
        AddSubHook(new SampActionObserver<nint, int>((_, result) => completed(result != 0)));
        InstallHook(address, new Native(HookProc));
    }

    private int HookProc(nint instance) => ProcessNative(instance, 0);
    protected override int InvokeOriginalFunction(nint instance) => OriginalFunction(instance);
}

internal readonly record struct SampIntActionArgs(nint Instance, int Value);

/// <summary>RequestClass and ExitVehicle take one four-byte argument, cleaned by ret 4.</summary>
internal sealed class SampIntActionHook : NativeHook<SampIntActionArgs, NoRetValue, SampIntActionHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint instance, int value);

    public SampIntActionHook(nint address, Action<int> completed)
    {
        AddSubHook(new SampActionObserver<SampIntActionArgs, NoRetValue>((args, _) => completed(args.Value)));
        InstallHook(address, new Native(HookProc));
    }

    private void HookProc(nint instance, int value) => ProcessNative(new(instance, value), default);

    protected override NoRetValue InvokeOriginalFunction(SampIntActionArgs args)
    {
        OriginalFunction(args.Instance, args.Value);
        return default;
    }
}

internal readonly record struct SampInteriorActionArgs(nint Instance, byte InteriorId);

/// <summary>ChangeInterior consumes the low byte of a four-byte stack slot, then ret 4.</summary>
internal sealed class SampInteriorActionHook : NativeHook<SampInteriorActionArgs, NoRetValue, SampInteriorActionHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint instance, byte interiorId);

    public SampInteriorActionHook(nint address, Action<byte> completed)
    {
        AddSubHook(new SampActionObserver<SampInteriorActionArgs, NoRetValue>((args, _) => completed(args.InteriorId)));
        InstallHook(address, new Native(HookProc));
    }

    private void HookProc(nint instance, byte interiorId) => ProcessNative(new(instance, interiorId), default);

    protected override NoRetValue InvokeOriginalFunction(SampInteriorActionArgs args)
    {
        OriginalFunction(args.Instance, args.InteriorId);
        return default;
    }
}

internal readonly record struct SampVehicleEntryActionArgs(nint Instance, int VehicleId, int Passenger);

/// <summary>EnterVehicle takes an int and a four-byte BOOL, then ret 8. Keep the original BOOL bits intact.</summary>
internal sealed class SampVehicleEntryActionHook : NativeHook<SampVehicleEntryActionArgs, NoRetValue, SampVehicleEntryActionHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint instance, int vehicleId, int passenger);

    public SampVehicleEntryActionHook(nint address, Action<SFSampVehicleEntryEvent> completed)
    {
        AddSubHook(new SampActionObserver<SampVehicleEntryActionArgs, NoRetValue>(
            (args, _) => completed(new(args.VehicleId, args.Passenger != 0))));
        InstallHook(address, new Native(HookProc));
    }

    private void HookProc(nint instance, int vehicleId, int passenger) => ProcessNative(new(instance, vehicleId, passenger), default);

    protected override NoRetValue InvokeOriginalFunction(SampVehicleEntryActionArgs args)
    {
        OriginalFunction(args.Instance, args.VehicleId, args.Passenger);
        return default;
    }
}
