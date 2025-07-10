using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe UpdateScoresPingsIpsList = delegate* unmanaged[Cdecl]<void*, void>;
public record struct UpdateScoresPingsIpsArgs(uint ParamsPtr);

public unsafe class UpdateScoresPingsIpsHook : JumpHook<UpdateScoresPingsIpsArgs, NoRetValue>
{
    private static UpdateScoresPingsIpsHook? _instance;
    public UpdateScoresPingsIpsHook() : base(
        stolenByteCount: 0xD,
        functionAddress: HookHelper.GetFunctionPtr("samp.dll", 0x103E0)
    ) => _instance = this;

    protected override void* InjectedFunction => (UpdateScoresPingsIpsList)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HookProc(void* paramsPtr)
    {
        if (_instance is null) throw new UnreachableException();

        _instance.Process(new((uint)paramsPtr));
    }

    protected override NoRetValue InvokeOriginalFunction(UpdateScoresPingsIpsArgs args)
    {
        ((UpdateScoresPingsIpsList)OriginalFunction)((void*)args.ParamsPtr);
        return default;
    }
    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}


