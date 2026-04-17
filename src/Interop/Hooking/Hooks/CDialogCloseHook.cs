using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

using unsafe CDialogCloseDirect = delegate* unmanaged[Thiscall]<void*, byte, void>;

public record struct CDialogCloseArgs(uint ThisPtr, byte DialogButton);

internal unsafe class CDialogCloseHook : NativeHook<CDialogCloseArgs, NoRetValue, CDialogCloseHook.CDialogCloseNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CDialogCloseNative(IntPtr thisPtr, byte dialogButton);

    private static CDialogCloseHook? _instance;

    public CDialogCloseHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Close), new CDialogCloseNative(HookProc));
    }

    private static void HookProc(IntPtr thisPtr, byte dialogButton)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new((uint)thisPtr, dialogButton));
    }

    protected override NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        using var _ = SuppressHook();
        ((CDialogCloseDirect)TargetAddress)((void*)args.ThisPtr, args.DialogButton);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
