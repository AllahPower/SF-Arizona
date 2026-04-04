using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public static class SFBootstrap
{
    private static SFSynchronizationContext? _sc;
    private static readonly NetworkDispatcher _dispatcher = new();

    public static RpcHandlerManager RpcHandlers => _dispatcher.IncomingRpcHandlers;
    public static OutgoingRpcManager OutgoingRpcHandlers => _dispatcher.OutgoingRpcHandlers;
    public static IncomingPacketManager IncomingPacketHandlers => _dispatcher.IncomingPacketHandlers;
    public static OutgoingPacketManager OutgoingPacketHandlers => _dispatcher.OutgoingPacketHandlers;
    public static IncomingAZVoiceControlManager IncomingAZVoiceControlHandlers => _dispatcher.IncomingAZVoiceControlHandlers;
    public static IncomingAZVoiceDataManager IncomingAZVoiceDataHandlers => _dispatcher.IncomingAZVoiceDataHandlers;

    public static void PostToMainThread(Action action)
    {
        _sc?.Post(x => ((Action)x!)(), action);
    }

    public static void ProcessException(Exception ex)
    {
        SFLog.Error(ex, "Unhandled library exception");

        try
        {
            CChat.Instance.AddEntry(EntryType.Chat, $"{ex.GetType()}: {ex.Message}", null, 0xFFFFFFFF, 0);
        }
        catch (Exception chatEx)
        {
            SFLog.Warn($"ProcessException fallback skipped chat output: {chatEx.GetType().Name}: {chatEx.Message}");
        }
    }

    public static void EnqueueIncomingRpc(int rpcId, byte[] packet, int payloadBitOffset, int payloadBitLength)
    {
        _dispatcher.EnqueueIncomingRpc(rpcId, packet, payloadBitOffset, payloadBitLength);
    }

    public static void EnqueueOutgoingRpc(int rpcId, byte[] packet, int dataBitLength)
    {
        _dispatcher.EnqueueOutgoingRpc(rpcId, packet, dataBitLength);
    }

    public static void EnqueueIncomingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueIncomingPacket(packetId, data, dataBitLength);
    }

    public static void EnqueueOutgoingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueOutgoingPacket(packetId, data, dataBitLength);
    }

    public static void EnqueueIncomingAZVoiceControl(int subId, byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueIncomingAZVoiceControl(subId, data, dataBitLength);
    }

    public static void EnqueueIncomingAZVoiceData(byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueIncomingAZVoiceData(data, dataBitLength);
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
        uint baseAddress = await GetSampDllBaseAddress();
        SFLog.Info($"samp.dll loaded at 0x{baseAddress:X8}");

        ValidateEnvironment();

        _ = HookManager.IncomingRpcPacket;
        SFLog.Info("IncomingRpc hook installed (pre-CNetGame).");

        await WhenCNetGameLoads(baseAddress);
        SFLog.Info("CNetGame is ready");

        _dispatcher.Reset();
        SF.Chat.RegisterRpcBindings(_dispatcher.IncomingRpcHandlers);
        _dispatcher.IncomingRpcHandlers.StartAll();

        InstallNetworkHooks();
        InstallSubHooks();

        SF.Keyboard.StartLoop();
        SFLog.Info("Keyboard loop started");

        PostToMainThread(main);
    }

    private static void ValidateEnvironment()
    {
        SampVersionInfo? env = SampEnvironment.Detect();
        if (env is null)
        {
            SFLog.Warn("SampEnvironment.Detect returned null — samp.dll vanished?");
            return;
        }

        SFLog.Info($"SA-MP detected: version={env.Version} EP=0x{env.EntryPointRva:X} SizeOfImage=0x{env.SizeOfImage:X} TimeDateStamp=0x{env.TimeDateStamp:X} SizeOfCode=0x{env.SizeOfCode:X}");
        if (!env.IsSupported)
            SFLog.Warn($"Unsupported SA-MP version (EP=0x{env.EntryPointRva:X}). SFSharp targets 0.3.7-R3. Hooks and offsets may be incorrect.");

        // sampfuncs.asi
        SampfuncsInfo sf = env.Sampfuncs;
        if (!sf.IsLoaded)
            SFLog.Warn("SAMPFUNCS not detected. sampfuncs.asi is not loaded — some features may be unavailable.");
        else if (sf.IsSupported)
            SFLog.Info($"SAMPFUNCS detected: {sf.VersionString} EP=0x{sf.EntryPointRva:X} SizeOfImage=0x{sf.SizeOfImage:X}");
        else
            SFLog.Warn($"Unsupported SAMPFUNCS version: {sf.VersionString ?? "unknown"} (EP=0x{sf.EntryPointRva:X}). SFSharp targets v5.5.0 rel.22.");

        // _chat.asi
        if (ModuleResolver.IsModuleLoaded("_chat.asi"))
        {
            if (CArizonaChat.IsAvailable)
                SFLog.Info("_chat.asi detected, Arizona chat interop available.");
            else
                SFLog.Warn("_chat.asi loaded but chat functions could not be resolved — patterns may have changed.");
        }
        else
        {
            SFLog.Info("_chat.asi not loaded, Arizona chat features disabled.");
        }

        // AZVoice.asi
        if (ModuleResolver.IsModuleLoaded("AZVoice.asi"))
        {
            if (IncomingAZVoicePacketHook.IsAvailable)
                SFLog.Info("AZVoice.asi detected, hook target resolved.");
            else
                SFLog.Warn("AZVoice.asi loaded but hook pattern not found — voice packet capture will be unavailable.");
        }
        else
        {
            SFLog.Info("AZVoice.asi not loaded, voice features disabled.");
        }
    }

    private static void InstallNetworkHooks()
    {
        _ = HookManager.OutgoingRpcPacket;
        _ = HookManager.OutgoingPacket;
        _ = HookManager.IncomingPacket;
        SFLog.Info("Network hooks installed: OutgoingRpc, OutgoingPacket, IncomingPacket.");

        if (HookManager.IncomingAZVoicePacket is not null)
            SFLog.Info("AZVoice incoming packet hook installed.");

        if (HookManager.IncomingAZVoiceRpc is not null)
            SFLog.Info("AZVoice incoming RPC hook installed.");
    }

    private static void InstallSubHooks()
    {
        HookManager.CDialogShow.AddSubHook(SF.Dialog);
        HookManager.CDialogHide.AddSubHook(SF.Dialog);
        HookManager.CDialogClose.AddSubHook(SF.Dialog);
        HookManager.CChatAddEntry.AddSubHook(SF.Chat);
        HookManager.CInputCommandSend.AddSubHook(SF.Chat);
        HookManager.UpdateScoresPingsIps.AddSubHook(SF.Players);
        SFLog.Info("Sub-hooks registered: Dialog, Chat, Input, Scoreboard.");
    }

    private static async Task<uint> GetSampDllBaseAddress()
    {
        while (true)
        {
            uint result = Win32.GetModuleHandle("samp.dll");
            if (result != 0)
            {
                return result;
            }

            await Task.Yield();
        }
    }

    private static async Task WhenCNetGameLoads(uint baseAddress)
    {
        SFLog.Info($"Waiting for CNetGame pointer at samp.dll+0x{SampOffsets.CNetGame.Instance:X8} from base 0x{baseAddress:X8}");
        while (!ModuleResolver.IsClassReady("samp.dll", SampOffsets.CNetGame.Instance))
        {
            await Task.Yield();
        }
    }
}
