using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Gta;

/// <summary>Resident, process-lifetime call-site owner. Chains the existing Idle target, including SA-MP's detour.</summary>
internal sealed unsafe class GtaProcessBridge
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Process();

    private readonly Process _callback;
    private readonly Process _original;
    private readonly SFGameClock _clock;
    private readonly nint _game;
    private readonly nint _site;
    private readonly int _displacement;

    public GtaProcessBridge(SFGameClock clock)
    {
        _clock = clock;
        _game = ModuleResolver.GetGameAddress(0);
        nint site = _game + GtaOffsets.ClockRva.ProcessCall;
        _site = site;
        if (!NativeMemoryValidator.IsReadable(site, 5) || *(byte*)site != 0xE8)
            throw new InvalidOperationException("GTA Idle Process call is not call rel32; no hook installed.");
        nint target = site + 5 + *(int*)(site + 1);
        if (!NativeMemoryValidator.IsReadable(target)) throw new InvalidOperationException("GTA Process target is unreadable.");
        _original = Marshal.GetDelegateForFunctionPointer<Process>(target);
        _callback = Run;
        nint callback = Marshal.GetFunctionPointerForDelegate(_callback);
        _displacement = unchecked((int)(callback - (site + 5)));
        if (!Win32.VirtualProtect((uint)(site + 1), 4, MemoryProtection.EXECUTE_READWRITE, out MemoryProtection old))
            throw new Win32Exception();
        try { *(int*)(site + 1) = _displacement; }
        finally
        {
            if (!Win32.VirtualProtect((uint)(site + 1), 4, old, out _)) SFLog.Warn("Failed to restore GTA Process page protection.");
            if (!Win32.FlushInstructionCache(-1, (uint)site, 5)) SFLog.Warn("Failed to flush GTA Process instruction cache.");
        }
        clock.IsAvailable = true;
    }

    internal void CheckOwnership()
    {
        if (_clock.IsAvailable && (*(byte*)_site != 0xE8 || *(int*)(_site + 1) != _displacement))
        {
            _clock.IsAvailable = false;
            SFLog.Warn("The GTA Process call site was replaced; GTA clock is unavailable. No automatic repatching will be attempted.");
        }
    }

    private void Run()
    {
        _original();
        try
        {
            _clock.Publish(*(uint*)(_game + GtaOffsets.ClockRva.FrameCounter),
                *(uint*)(_game + GtaOffsets.ClockRva.Milliseconds),
                *(float*)(_game + GtaOffsets.ClockRva.TimeStep),
                *(byte*)(_game + GtaOffsets.ClockRva.CodePause) != 0 || *(byte*)(_game + GtaOffsets.ClockRva.UserPause) != 0);
        }
        catch (Exception ex) { HookRuntime.ReportException?.Invoke(ex); }
    }
}
