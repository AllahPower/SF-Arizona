using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public static class SFBootstrap
{
    private const int MaxRpcDispatchPerTick = 24;

    private static SFSynchronizationContext? _sc;
    private static RpcHandlerManager _rpcHandlers = new();
    private static OutgoingRpcManager _outgoingRpcHandlers = new();
    private static readonly ConcurrentQueue<(int RpcId, byte[] Packet, int PayloadBitOffset, int PayloadBitLength)> _pendingRpcs = new();
    private static readonly ConcurrentQueue<(int RpcId, byte[] Packet, int DataBitLength)> _pendingOutgoingRpcs = new();
    private static int _rpcDispatchScheduled;
    private static int _outgoingRpcDispatchScheduled;

    public static RpcHandlerManager RpcHandlers => _rpcHandlers;
    public static OutgoingRpcManager OutgoingRpcHandlers => _outgoingRpcHandlers;

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
        _pendingRpcs.Enqueue((rpcId, packet, payloadBitOffset, payloadBitLength));
        ScheduleRpcDispatch();
    }

    public static void EnqueueOutgoingRpc(int rpcId, byte[] packet, int dataBitLength)
    {
        _pendingOutgoingRpcs.Enqueue((rpcId, packet, dataBitLength));
        ScheduleOutgoingRpcDispatch();
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

        _rpcHandlers = new RpcHandlerManager();
        _outgoingRpcHandlers = new OutgoingRpcManager();
        SF.Chat.RegisterRpcBindings(_rpcHandlers);
        _rpcHandlers.StartAll();

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

    private static void ScheduleRpcDispatch()
    {
        if (Interlocked.CompareExchange(ref _rpcDispatchScheduled, 1, 0) != 0)
        {
            return;
        }

        PostToMainThread(ProcessIncomingRpcBatch);
    }

    private static void ProcessIncomingRpcBatch()
    {
        int processed = 0;
        while (processed < MaxRpcDispatchPerTick && _pendingRpcs.TryDequeue(out (int RpcId, byte[] Packet, int PayloadBitOffset, int PayloadBitLength) item))
        {
            _rpcHandlers.DispatchIncoming(item.RpcId, item.Packet, item.PayloadBitOffset, item.PayloadBitLength);
            processed++;
        }

        if (_pendingRpcs.IsEmpty)
        {
            Interlocked.Exchange(ref _rpcDispatchScheduled, 0);
            if (!_pendingRpcs.IsEmpty)
            {
                ScheduleRpcDispatch();
            }

            return;
        }

        PostToMainThread(ProcessIncomingRpcBatch);
    }

    private static void ScheduleOutgoingRpcDispatch()
    {
        if (Interlocked.CompareExchange(ref _outgoingRpcDispatchScheduled, 1, 0) != 0)
        {
            return;
        }

        PostToMainThread(ProcessOutgoingRpcBatch);
    }

    private static void ProcessOutgoingRpcBatch()
    {
        int processed = 0;
        while (processed < MaxRpcDispatchPerTick && _pendingOutgoingRpcs.TryDequeue(out (int RpcId, byte[] Packet, int DataBitLength) item))
        {
            _outgoingRpcHandlers.Dispatch(item.RpcId, item.Packet, item.DataBitLength);
            processed++;
        }

        if (_pendingOutgoingRpcs.IsEmpty)
        {
            Interlocked.Exchange(ref _outgoingRpcDispatchScheduled, 0);
            if (!_pendingOutgoingRpcs.IsEmpty)
            {
                ScheduleOutgoingRpcDispatch();
            }

            return;
        }

        PostToMainThread(ProcessOutgoingRpcBatch);
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
