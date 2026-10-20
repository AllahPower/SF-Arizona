using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Hooks;

internal unsafe class OutgoingPacketHook : NativeHook<nint, bool, OutgoingPacketHook.SendBitStreamNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate bool SendBitStreamNative(
        nint thisPtr,
        nint bitStream,
        int priority,
        int reliability,
        byte orderingChannel);

    private static OutgoingPacketHook? _instance;
    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    internal OutgoingPacketHook(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        _instance = this;
        nint targetAddress = ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RakClientRuntime.SendBitStream);
        InstallHook(targetAddress, new SendBitStreamNative(HookProc));
    }

    private static unsafe bool HookProc(
        nint thisPtr,
        nint bitStream,
        int priority,
        int reliability,
        byte orderingChannel)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        if (bitStream != 0)
        {
            if (RakNetHookPayloadReader.TryReadPacketBitStream((CRakNetBitStream*)bitStream, out int packetId, out int bitsUsed, out ReadOnlySpan<byte> payload))
            {
                byte* data = ((CRakNetBitStream*)bitStream)->Data;

                if (_instance!._filters.OutgoingPacket.HasFilters &&
                    _instance!._filters.OutgoingPacket.ShouldCancel(packetId, data, bitsUsed))
                {
                    return true;
                }

                if (_instance!._dispatcher.OutgoingPacketHandlers.HasSubscribers(packetId))
                {
                    _instance!._dispatcher.EnqueueOutgoingPacket(packetId, payload.ToArray(), bitsUsed);
                }
            }
        }

        return _instance.OriginalFunction(thisPtr, bitStream, priority, reliability, orderingChannel);
    }

    protected override bool InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
