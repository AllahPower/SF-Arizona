using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct CDialogCloseArgs(uint ThisPtr, byte DialogButton);

internal unsafe class CDialogCloseHook : NativeHook<CDialogCloseArgs, NoRetValue, CDialogCloseHook.CDialogCloseNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CDialogCloseNative(IntPtr thisPtr, byte dialogButton);

    public CDialogCloseHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Close), new CDialogCloseNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr, byte dialogButton)
    {
        try { Process(new((uint)thisPtr, dialogButton)); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }
    }

    protected override NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        OriginalFunction((nint)args.ThisPtr, args.DialogButton);
        return default;
    }

}
