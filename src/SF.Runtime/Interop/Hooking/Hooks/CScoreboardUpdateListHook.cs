using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct UpdateScoresPingsIpsArgs(uint ParamsPtr);

internal unsafe class UpdateScoresPingsIpsHook : NativeHook<UpdateScoresPingsIpsArgs, NoRetValue, UpdateScoresPingsIpsHook.UpdateScoresPingsIpsNative>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void UpdateScoresPingsIpsNative(IntPtr paramsPtr);

    public UpdateScoresPingsIpsHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CScoreboard.UpdateScoresPingsIps), new UpdateScoresPingsIpsNative(HookProc));
    }

    private void HookProc(IntPtr paramsPtr)
    {
        try { Process(new((uint)paramsPtr)); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }
    }

    protected override NoRetValue InvokeOriginalFunction(UpdateScoresPingsIpsArgs args)
    {
        OriginalFunction((nint)args.ParamsPtr);
        return default;
    }

}
