using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

using unsafe CDialogCloseSampfuncsDirect = delegate* unmanaged[Cdecl]<int, int>;

internal unsafe class CDialogCloseSampfuncsHook : NativeHook<CDialogCloseArgs, NoRetValue, CDialogCloseSampfuncsHook.CDialogCloseSampfuncsNative>, IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int CDialogCloseSampfuncsNative(int dialogButton);

    private static CDialogCloseSampfuncsHook? _instance;

    public CDialogCloseSampfuncsHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("sampfuncs.asi", SampOffsets.SampFuncs.CDialogClose), new CDialogCloseSampfuncsNative(HookProc));
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
        ((CDialogCloseSampfuncsDirect)TargetAddress)(args.DialogButton);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
