using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe CDialogHideDirect = delegate* unmanaged[Thiscall]<void*, void>;

public record struct CDialogHideArgs(uint ThisPtr);

internal unsafe class CDialogHideHook : NativeHook<CDialogHideArgs, NoRetValue, CDialogHideHook.CDialogHideNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CDialogHideNative(IntPtr thisPtr);

    private static CDialogHideHook? _instance;

    public CDialogHideHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Hide), new CDialogHideNative(HookProc));
    }

    private static void HookProc(IntPtr thisPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new((uint)thisPtr));
    }

    protected override NoRetValue InvokeOriginalFunction(CDialogHideArgs args)
    {
        using var _ = SuppressHook();
        ((CDialogHideDirect)TargetAddress)((void*)args.ThisPtr);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
