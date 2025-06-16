using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe static partial class SFCore
{
    private static SFSynchronizationContext? _sc;
    public static void PostToMainLoop(SendOrPostCallback callback)
    {
        if (_sc == null) throw new UnreachableException();
        _sc.Post(callback, null);
    }

    public static int Init(nint exportsPtr, Action mainMethod)
    {
        var exports = (CSharpExports*)exportsPtr;
        _exports = *exports;
        if(_exports.Version != sizeof(CSharpExports))
        {
            return 1;
        }

        _sc = new SFSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(_sc);
        exports->MainLoop = &MainLoop;

        RegisterDialogCallback(&SFDialog.DialogCallback);
        SF.InstallChatHook();

        try { mainMethod(); } catch(Exception ex) { LogException(ex); }
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void MainLoop()
    {
        SFSynchronizationContext.LoopProc();
    }

    internal static void LogException(Exception ex)
    {
        LogToChat($"{ex.GetType()}: {ex.Message}");
    }
}