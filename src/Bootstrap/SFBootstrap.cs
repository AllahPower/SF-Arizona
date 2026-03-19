using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public static class SFBootstrap
{
    private static SFSynchronizationContext? _sc;
    private static readonly RpcDispatcher _rpcDispatcher = new();

    public static RpcHandlerManager RpcHandlers => _rpcDispatcher.IncomingHandlers;
    public static OutgoingRpcManager OutgoingRpcHandlers => _rpcDispatcher.OutgoingHandlers;

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
        _rpcDispatcher.EnqueueIncoming(rpcId, packet, payloadBitOffset, payloadBitLength);
    }

    public static void EnqueueOutgoingRpc(int rpcId, byte[] packet, int dataBitLength)
    {
        _rpcDispatcher.EnqueueOutgoing(rpcId, packet, dataBitLength);
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

        _rpcDispatcher.Reset();
        SF.Chat.RegisterRpcBindings(_rpcDispatcher.IncomingHandlers);
        _rpcDispatcher.IncomingHandlers.StartAll();

        _ = HookManager.OutgoingRpcPacket;
        SFLog.Info("Outgoing RPC hook prepared");

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
