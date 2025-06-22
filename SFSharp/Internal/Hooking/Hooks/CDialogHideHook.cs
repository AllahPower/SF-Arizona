using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

public unsafe class CDialogHideHook : Hook<CDialogHideArgs, NoRetValue>, IDisposable
{
    private const int StolenBytesCount = 5;

    private static CDialogHideHook? _instance;
    private readonly delegate* unmanaged[Thiscall]<void*, void> _trampolinePtr;
    private readonly uint _functionAddress;

    public CDialogHideHook() : base(BaseFunction)
    {
        if (_instance is not null) throw new InvalidOperationException();

        _functionAddress = HookHelper.GetFunctionPtr("samp.dll", 0x6F860);
        _trampolinePtr = (delegate* unmanaged[Thiscall]<void*, void>)HookHelper.InstallJumpHook(
            _functionAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Thiscall]<void*, void>)&HookedFunction
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void HookedFunction(void* thisPtr)
    {
        if (_instance is null) throw new UnreachableException();

        try
        {
            _instance.Process(new((uint)thisPtr));
        }
        catch (Exception ex)
        {
            SFCore.LogException(ex);
        }
    }

    private static unsafe NoRetValue BaseFunction(CDialogHideArgs args)
    {
        if (_instance is null) throw new UnreachableException();

        _instance._trampolinePtr((void*)args.ThisPtr);
        return default;
    }

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveJumpHook(_functionAddress, StolenBytesCount, (uint)(delegate* unmanaged[Thiscall]<void*, void>)&HookedFunction);
        _instance = null;
    }
}

public record struct CDialogHideArgs(uint ThisPtr);