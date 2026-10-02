using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Hooks;

internal unsafe class IncomingPacketHook : NativeHook<nint, nint, IncomingPacketHook.ReceiveNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate nint ReceiveNative(nint thisPtr);

    private static IncomingPacketHook? _instance;
    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    internal IncomingPacketHook(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        _instance = this;
        nint targetAddress = ModuleResolver.ResolveVTableFunction(
            "samp.dll",
            SampOffsets.CNetGame.Instance,
            SampOffsets.CNetGame.RakClient,
            SampOffsets.RakClientVTable.Receive);
        InstallHook(targetAddress, new ReceiveNative(HookProc));
    }

    private static unsafe nint HookProc(nint thisPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        nint packetPtr = _instance.OriginalFunction(thisPtr);

        if (packetPtr != 0)
        {
            int bitSize = *(int*)(packetPtr + SampOffsets.RakNetPacket.BitSize);
            byte* data = *(byte**)(packetPtr + SampOffsets.RakNetPacket.Data);

            if (data != null && bitSize >= 8)
            {
                int packetId = data[0];

                if (_instance!._filters.IncomingPacket.HasFilters &&
                    _instance!._filters.IncomingPacket.ShouldCancel(packetId, data, bitSize))
                {
                    CNetGame.GetRakClient()->DeallocatePacket((CRakNetPacket*)packetPtr);
                    return 0;
                }

                if (_instance!._dispatcher.IncomingPacketHandlers.HasSubscribers(packetId))
                {
                    int dataByteLength = (bitSize + 7) / 8;
                    byte[] packet = new byte[dataByteLength];
                    fixed (byte* dst = packet)
                    {
                        Buffer.MemoryCopy(data, dst, dataByteLength, dataByteLength);
                    }

                    _instance!._dispatcher.EnqueueIncomingPacket(packetId, packet, bitSize);
                }
            }
        }

        return packetPtr;
    }

    protected override nint InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
