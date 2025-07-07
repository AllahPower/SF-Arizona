using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe CDialogClose = delegate*unmanaged[Thiscall]<void*, byte, void>;
public record struct CDialogCloseArgs(uint ThisPtr, byte DialogButton);

public unsafe class CDialogCloseHook : JumpHook<CDialogCloseArgs, NoRetValue>
{
    private static CDialogCloseHook? _instance;
    public CDialogCloseHook() : base(
        stolenByteCount: 6,
        targetFunctionModule: "samp.dll",
        targetFunctionOffset: 0x70630
    ) => _instance = this;

    protected override void* InjectedFunction => (CDialogClose)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static void HookProc(void* thisPtr, byte dialogButton)
    {
        if (_instance is null) throw new UnreachableException();

        _instance.Process(new((uint)thisPtr, dialogButton));
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        ((CDialogClose)OriginalFunction)((void*)args.ThisPtr, args.DialogButton);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}

