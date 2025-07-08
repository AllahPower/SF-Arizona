using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

using unsafe CDialogHide = delegate*unmanaged[Thiscall]<void*, void>;
public record struct CDialogHideArgs(uint ThisPtr);

public unsafe class CDialogHideHook : JumpHook<CDialogHideArgs, NoRetValue>
{
    private static CDialogHideHook? _instance;
    public CDialogHideHook() : base(
        stolenByteCount: 5,
        functionAddress: HookHelper.GetFunctionPtr("samp.dll", 0x6F860)
    ) => _instance = this;

    protected override void* InjectedFunction => (CDialogHide)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void HookProc(void* thisPtr)
    {
        if (_instance is null) throw new UnreachableException();
        
        _instance.Process(new((uint)thisPtr));
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogHideArgs args)
    {
        ((CDialogHide)OriginalFunction)((void*)args.ThisPtr);
        return default;
    }
}

