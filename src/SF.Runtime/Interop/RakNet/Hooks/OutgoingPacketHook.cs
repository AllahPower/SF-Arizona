using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet.Hooks;

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

    public OutgoingPacketHook()
    {
        _instance = this;
        nint targetAddress = ModuleResolver.ResolveVTableFunction(
            "samp.dll",
            SampOffsets.CNetGame.Instance,
            SampOffsets.CNetGame.RakClient,
            SampOffsets.RakClientVTable.Send_BitStream);
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
            int bitsUsed = *(int*)(bitStream + SampOffsets.RakNetBitStream.NumberOfBitsUsed);
            byte* data = *(byte**)(bitStream + SampOffsets.RakNetBitStream.Data);

            if (data != null && bitsUsed >= 8)
            {
                int packetId = data[0];

                if (SFBootstrap.OutgoingPacketFilters.HasFilters &&
                    SFBootstrap.OutgoingPacketFilters.ShouldCancel(packetId, data, bitsUsed))
                {
                    return true;
                }

                if (SFBootstrap.OutgoingPacketHandlers.HasSubscribers(packetId))
                {
                    int dataByteLength = (bitsUsed + 7) / 8;
                    byte[] packet = new byte[dataByteLength];
                    fixed (byte* dst = packet)
                    {
                        Buffer.MemoryCopy(data, dst, dataByteLength, dataByteLength);
                    }

                    SFBootstrap.EnqueueOutgoingPacket(packetId, packet, bitsUsed);
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
