using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Hooks;

internal sealed unsafe class OutgoingDataPacketHook : NativeHook<nint, bool, OutgoingDataPacketHook.SendDataNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate bool SendDataNative(nint thisPtr, byte* data, int length, int priority, int reliability, byte orderingChannel);

    private static OutgoingDataPacketHook? _instance;
    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    internal OutgoingDataPacketHook(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RakClientRuntime.SendData), new SendDataNative(HookProc));
    }

    private static bool HookProc(nint thisPtr, byte* data, int length, int priority, int reliability, byte orderingChannel)
    {
        OutgoingDataPacketHook instance = _instance ?? throw new UnreachableException();
        if (data != null && length > 0 && length <= int.MaxValue / 8)
        {
            int packetId = data[0];
            int bitLength = length * 8;
            if (instance._filters.OutgoingPacket.HasFilters && instance._filters.OutgoingPacket.ShouldCancel(packetId, data, bitLength))
            {
                return true;
            }

            if (instance._dispatcher.OutgoingPacketHandlers.HasSubscribers(packetId))
            {
                instance._dispatcher.EnqueueOutgoingPacket(packetId, new ReadOnlySpan<byte>(data, length).ToArray(), bitLength);
            }
        }

        return instance.OriginalFunction(thisPtr, data, length, priority, reliability, orderingChannel);
    }

    protected override bool InvokeOriginalFunction(nint args) => throw new NotSupportedException();

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
