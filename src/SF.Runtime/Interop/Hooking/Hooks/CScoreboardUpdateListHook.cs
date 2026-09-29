using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking;

using unsafe UpdateScoresPingsIpsDirect = delegate* unmanaged[Cdecl]<void*, void>;

public record struct UpdateScoresPingsIpsArgs(uint ParamsPtr);

internal unsafe class UpdateScoresPingsIpsHook : NativeHook<UpdateScoresPingsIpsArgs, NoRetValue, UpdateScoresPingsIpsHook.UpdateScoresPingsIpsNative>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void UpdateScoresPingsIpsNative(IntPtr paramsPtr);

    private static UpdateScoresPingsIpsHook? _instance;

    public UpdateScoresPingsIpsHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CScoreboard.UpdateScoresPingsIps), new UpdateScoresPingsIpsNative(HookProc));
    }

    private static void HookProc(IntPtr paramsPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new((uint)paramsPtr));
    }

    protected override NoRetValue InvokeOriginalFunction(UpdateScoresPingsIpsArgs args)
    {
        using var _ = SuppressHook();
        ((UpdateScoresPingsIpsDirect)TargetAddress)((void*)args.ParamsPtr);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
