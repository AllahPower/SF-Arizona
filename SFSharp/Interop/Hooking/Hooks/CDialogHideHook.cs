using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
public unsafe delegate void CDialogHide(void* thisPtr);

public unsafe class CDialogHideHook : JumpHook<CDialogHideArgs, NoRetValue, CDialogHide>
{
    public CDialogHideHook() : base(
        stolenByteCount: 5,
        targetFunctionModule: "samp.dll",
        targetFunctionOffset: 0x6F860)
    {
    }

    protected override CDialogHide HookedFunction => HookProc;
    private unsafe void HookProc(void* thisPtr)
    {
        Process(new((uint)thisPtr));
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogHideArgs args)
    {
        Trampoline((void*)args.ThisPtr);
        return default;
    }
}

public record struct CDialogHideArgs(uint ThisPtr);