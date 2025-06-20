using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

public unsafe class CDialogCloseHook : Hook<CDialogCloseArgs>, IDisposable
{
    private const int StolenBytesCount = 6; // This is not tested, and won't be until we get running without SF

    private static CDialogCloseHook? _instance;
    private readonly delegate* unmanaged[Thiscall]<void*, byte, void> _trampolinePtr;
    private readonly uint _functionAddress;

    public CDialogCloseHook() : base(BaseFunction)
    {
        if (_instance is not null) throw new InvalidOperationException();

        _functionAddress = HookHelper.GetFunctionPtr("samp.dll", 0x70630);
        _trampolinePtr = (delegate* unmanaged[Thiscall]<void*, byte, void>)HookHelper.InstallJumpHook(
            _functionAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Thiscall]<void*, byte, void>)&HookedFunction
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void HookedFunction(void* thisPtr, byte dialogButton)
    {
        if(_instance is null) throw new UnreachableException();

        _instance.Process(new((uint)thisPtr, dialogButton));
    }

    private static unsafe void BaseFunction(CDialogCloseArgs args)
    {
        if (_instance is null) throw new UnreachableException();

        _instance._trampolinePtr((void*)args.ThisPtr, args.DialogButton);
    }

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveJumpHook(_functionAddress, StolenBytesCount, (uint)(delegate* unmanaged[Thiscall]<void*, byte, void>)&HookedFunction);
        _instance = null;
    }
}

public record CDialogCloseArgs(uint ThisPtr, byte DialogButton);