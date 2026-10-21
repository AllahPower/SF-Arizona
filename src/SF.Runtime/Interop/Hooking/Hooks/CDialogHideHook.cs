using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct CDialogHideArgs(uint ThisPtr);

internal unsafe class CDialogHideHook : NativeHook<CDialogHideArgs, NoRetValue, CDialogHideHook.CDialogHideNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CDialogHideNative(IntPtr thisPtr);

    public CDialogHideHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Hide), new CDialogHideNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr)
    {
        try { Process(new((uint)thisPtr)); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }
    }

    protected override NoRetValue InvokeOriginalFunction(CDialogHideArgs args)
    {
        OriginalFunction((nint)args.ThisPtr);
        return default;
    }

}
