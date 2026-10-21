using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Hooks;

internal unsafe class IncomingPacketHook : NativeHook<nint, nint, IncomingPacketHook.ReceiveNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate nint ReceiveNative(nint thisPtr);

    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    internal IncomingPacketHook(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        nint targetAddress = ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RakClientRuntime.Receive);
        InstallHook(targetAddress, new ReceiveNative(HookProc));
    }

    private nint HookProc(nint thisPtr)
    {
        nint packetPtr;
        try { packetPtr = OriginalFunction(thisPtr); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); return 0; }

        try
        {
            if (packetPtr != 0)
            {
                int bitSize = *(int*)(packetPtr + SampOffsets.RakNetPacket.BitSize);
                byte* data = *(byte**)(packetPtr + SampOffsets.RakNetPacket.Data);

                if (data != null && bitSize >= 8)
                {
                    int packetId = data[0];

                    if (_filters.IncomingPacket.HasFilters &&
                        _filters.IncomingPacket.ShouldCancel(packetId, data, bitSize))
                    {
                        nint cancelledPacket = packetPtr;
                        packetPtr = 0;
                        ((CRakClientInterface*)thisPtr)->DeallocatePacket((CRakNetPacket*)cancelledPacket);
                        return 0;
                    }

                    if (_dispatcher.IncomingPacketHandlers.HasSubscribers(packetId))
                    {
                        int dataByteLength = (bitSize + 7) / 8;
                        byte[] packet = new byte[dataByteLength];
                        fixed (byte* dst = packet)
                        {
                            Buffer.MemoryCopy(data, dst, dataByteLength, dataByteLength);
                        }

                        _dispatcher.EnqueueIncomingPacket(packetId, packet, bitSize);
                    }
                }
            }
        }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }

        return packetPtr;
    }

    protected override nint InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

}