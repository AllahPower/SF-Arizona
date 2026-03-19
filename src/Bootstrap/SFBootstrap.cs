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

        _ = HookManager.IncomingRpcPacket;
        SFLog.Info("Incoming RPC hook prepared");

        await WhenCNetGameLoads(baseAddress);
        SFLog.Info("CNetGame is ready");

        _dispatcher.Reset();
        SF.Chat.RegisterRpcBindings(_dispatcher.IncomingRpcHandlers);
        _dispatcher.IncomingRpcHandlers.StartAll();

        _ = HookManager.OutgoingRpcPacket;
        SFLog.Info("Outgoing RPC hook prepared");

        _ = HookManager.OutgoingPacket;
        SFLog.Info("Outgoing packet hook prepared");

        _ = HookManager.IncomingPacket;
        SFLog.Info("Incoming packet hook prepared");

        HookManager.CDialogShow.AddSubHook(SF.Dialog);
        HookManager.CDialogHide.AddSubHook(SF.Dialog);
        HookManager.CDialogClose.AddSubHook(SF.Dialog);
        HookManager.CChatAddEntry.AddSubHook(SF.Chat);
        HookManager.CInputCommandSend.AddSubHook(SF.Chat);
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
