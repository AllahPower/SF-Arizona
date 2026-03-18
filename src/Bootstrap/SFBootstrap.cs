using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public static class SFBootstrap
{
    private static SFSynchronizationContext? _sc;

    public static void PostToMainThread(Action action)
    {
        SFLog.Info($"PostToMainThread scheduled action={action.Method.DeclaringType?.Name}.{action.Method.Name}");
        _sc?.Post(x => ((Action)x!)(), action);
    }

    public static void ProcessException(Exception ex)
    {
        SFLog.Error(ex, "Unhandled library exception");
        CChat.Instance.AddEntry(EntryType.Chat, $"{ex.GetType()}: {ex.Message}", null, 0xFFFFFFFF, 0);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "WinMainLoop")]
    public static void WinMainLoop() => WinMainLoopCore(Program.Main);

    public static void WinMainLoopCore(Action main)
    {
        if (_sc is null)
        {
            SFLog.Info("WinMainLoopCore first entry");
            _sc = new SFSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_sc);
            SFMain(main);
        }

        _sc.ProcLoop();
    }

    private static async void SFMain(Action main)
    {
        SFLog.Info("SFMain started");
        var baseAddress = await GetSampDllBaseAddress();
        SFLog.Info($"samp.dll loaded at 0x{baseAddress:X8}");
        await WhenCNetGameLoads(baseAddress);
        SFLog.Info("CNetGame is ready");

        await Task.Yield();

        HookManager.CDialogShow.AddSubHook(SF.Dialog);
        HookManager.CDialogHide.AddSubHook(SF.Dialog);
        HookManager.CDialogClose.AddSubHook(SF.Dialog);
        HookManager.CChatAddEntry.AddSubHook(SF.Chat);
        HookManager.CInputSend.AddSubHook(SF.Chat);
        HookManager.UpdateScoresPingsIps.AddSubHook(SF.Players);
        SFLog.Info("All sub-hooks registered");

        SF.Keyboard.StartLoop();
        SFLog.Info("Keyboard loop started");

        PostToMainThread(main);
    }

    private static async Task<uint> GetSampDllBaseAddress()
    {
        while (true)
        {
            var result = Win32.GetModuleHandle("samp.dll");
            if (result != 0)
            {
                return result;
            }

            await Task.Yield();
        }
    }

    private static async Task WhenCNetGameLoads(uint baseAddress)
    {
        SFLog.Info($"Waiting for CNetGame pointer at samp.dll+0x26E8DC from base 0x{baseAddress:X8}");
        while (!ModuleResolver.IsClassReady("samp.dll", 0x26E8DC))
        {
            await Task.Yield();
        }
    }
}
