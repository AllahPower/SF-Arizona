using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe static partial class SFCore
{
    internal static SFSynchronizationContext? _sc;
    internal static void PostToMainLoop(Action callback)
    {
        if (_sc == null) throw new UnreachableException();
        _sc.Post(static obj => ((Action)obj!)(), callback);
    }

    internal static void LogException(Exception ex)
    {
        LogToChat($"{ex.GetType()}: {ex.Message}");
    }

    private static bool initialized = false;
    private static uint baseAddress = 0;

    private static unsafe bool IsSampInitialized()
    {
        if (baseAddress == 0)
        {
            baseAddress = Win32.GetModuleHandle("samp.dll");
        }

        if (baseAddress == 0) return false;
        // Checking if CNetGame is initialized
        var chatPtr = (uint**)(baseAddress + 0x26EB94);
        if(*chatPtr == null) return false;
        if(**chatPtr == 0) return false;


        return true;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "WinMainLoop")]
    public static void WinMainLoop()
    {
        if (!initialized)
        {
            if (!IsSampInitialized()) return;

            _sc = new SFSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_sc);

            SF.InstallChatHook();
            SF.StartKeyboardLoop();
            DialogManager.InstallHooks();

            initialized = true;
            try { Program.Main(); } catch (Exception ex) { LogException(ex); }
            return;
        }

        _sc!.ProcLoop();
    }
}