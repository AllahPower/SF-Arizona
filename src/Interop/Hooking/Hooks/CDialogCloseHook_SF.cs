using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

using unsafe CDialogCloseSfDirect = delegate* unmanaged[Cdecl]<int, int>;

internal unsafe class CDialogCloseHook_SF : NativeHook<CDialogCloseArgs, NoRetValue, CDialogCloseHook_SF.CDialogCloseSfNative>, IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int CDialogCloseSfNative(int dialogButton);

    private static CDialogCloseHook_SF? _instance;

    public CDialogCloseHook_SF()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("sampfuncs.asi", SampOffsets.SampFuncs.CDialogClose), new CDialogCloseSfNative(HookProc));
    }

    private static int HookProc(int dialogButton)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new(0, (byte)dialogButton));
        return 0;
    }

    protected override NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        using var _ = SuppressHook();
        ((CDialogCloseSfDirect)TargetAddress)(args.DialogButton);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
