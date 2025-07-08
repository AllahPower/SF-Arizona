using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using unsafe CDialogClose_SF = delegate* unmanaged[Cdecl]<int, int>;

namespace SFSharp;

public unsafe class CDialogCloseHook_SF : CallHook<CDialogCloseArgs, NoRetValue>, IDisposable
{
    private static CDialogCloseHook_SF? _instance;

    public CDialogCloseHook_SF() : base(
        stolenByteCount: 5,
        callSiteAddress: HookHelper.GetFunctionPtr("sampfuncs.asi", 0x8681E),
        callTargetAddress: HookHelper.GetFunctionPtr("sampfuncs.asi", 0x8680F)
    ) => _instance = this;

    protected override void* InjectedFunction => (CDialogClose_SF)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe int HookProc(int dialogButton)
    {
        if (_instance is null) throw new UnreachableException();

        _instance.Process(new(0, (byte)dialogButton));
        return 0; // Unused
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        ((CDialogClose_SF)OriginalFunction)(args.DialogButton);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}