using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
public unsafe delegate void CDialogClose(void* thisPtr, byte dialogButton);

public unsafe class CDialogCloseHook : JumpHook<CDialogCloseArgs, NoRetValue, CDialogClose>
{
    public CDialogCloseHook() : base(
        stolenByteCount: 6,
        targetFunctionModule: "samp.dll",
        targetFunctionOffset: 0x70630)
    { }

    protected override CDialogClose HookedFunction => HookProc;
    private void HookProc(void* thisPtr, byte dialogButton)
    {
        Process(new((uint)thisPtr, dialogButton));
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        Trampoline((void*)args.ThisPtr, args.DialogButton);
        return default;
    }
}

public record struct CDialogCloseArgs(uint ThisPtr, byte DialogButton);