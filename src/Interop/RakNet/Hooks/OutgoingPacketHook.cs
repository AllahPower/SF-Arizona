using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

// Hook target is resolved from the live RakClient vtable.
// Current live mapping recovered from the process: slot 6 = Send(BitStream), slot 7 = Send(data), slot 8 = Receive.
internal unsafe class OutgoingPacketHook : NativeHook<nint, bool, OutgoingPacketHook.SendPacketBitStreamNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate bool SendPacketBitStreamNative(
        nint thisPtr,
        nint bitStream,
        int priority,
        int reliability,
        int orderingChannel);

    private static OutgoingPacketHook? _instance;

    public OutgoingPacketHook()
    {
        _instance = this;
        nint targetAddress = ModuleResolver.ResolveVTableFunction(
            "samp.dll",
            SampOffsets.CNetGame.Instance,
            SampOffsets.CNetGame.RakClient,
            SampOffsets.RakClientVTable.Send_BitStream);
        InstallHook(targetAddress, new SendPacketBitStreamNative(HookProc));
    }

    private static unsafe bool HookProc(
        nint thisPtr,
        nint bitStream,
        int priority,
        int reliability,
        int orderingChannel)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        if (bitStream != 0)
        {
            int bitLength = *(int*)(bitStream + SampOffsets.RakNetBitStream.NumberOfBitsUsed);
            byte* data = *(byte**)(bitStream + SampOffsets.RakNetBitStream.Data);

            if (data != null && bitLength >= 8)
            {
                int packetId = data[0];
                if (SFBootstrap.OutgoingPacketHandlers.HasSubscribers(packetId))
                {
                    int byteLength = (bitLength + 7) / 8;
                    byte[] packet = new byte[byteLength];
                    fixed (byte* dst = packet)
                    {
                        Buffer.MemoryCopy(data, dst, byteLength, byteLength);
                    }

                    SFBootstrap.EnqueueOutgoingPacket(packetId, packet, bitLength);
                }
            }
        }

        return _instance.OriginalFunction(
            thisPtr,
            bitStream,
            priority,
            reliability,
            orderingChannel);
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
