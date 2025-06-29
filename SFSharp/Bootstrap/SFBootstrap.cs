using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public static class SFBootstrap
{
    private static SFSynchronizationContext? _sc;
    public static void PostToMainThread(Action action) => _sc?.Post(x => ((Action)x!)(), action);
    public static void ProcessException(Exception ex) => CChat.Instance.AddEntry(EntryType.Debug, $"{ex.GetType()}: {ex.Message}", null, 0xFFFFFFFF, 0);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "WinMainLoop")]
    public static void WinMainLoop() => WinMainLoopCore(Program.Main);

    public static void WinMainLoopCore(Action main)
    {
        if (_sc is null)
        {
            _sc = new SFSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_sc);
            SFMain(main);
        }
        _sc.ProcLoop();
    }

    private static async void SFMain(Action main)
    {
        var baseAddress = await GetSampDllBaseAddress();
        await WhenCNetGameLoads(baseAddress);

        await Task.Yield();

        void Initialize(ISFComponent component) => component.Initialize();
        Initialize(SF.Dialog);
        Initialize(SF.Keyboard);
        Initialize(SF.Chat);

        PostToMainThread(main);
    }

    private static async Task<uint> GetSampDllBaseAddress()
    {
        while(true)
        {
            var result = Win32.GetModuleHandle("samp.dll");
            if (result != 0) return result;
            await Task.Yield();
        }
    }

    private static async Task WhenCNetGameLoads(uint baseAddress)
    {
        while(!HookHelper.IsClassReady("samp.dll", 0x26EB94))
        {
            await Task.Yield();
        }
    }
}
