using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

internal static unsafe class RakNetHookPayloadReader
{
    public static bool TryReadPacket(CRakNetPacket* packet, out int packetId, out int bitLength, out ReadOnlySpan<byte> data)
    {
        packetId = 0;
        bitLength = 0;
        data = default;

        if (packet == null)
        {
            return false;
        }

        return TryReadPacketData(packet->Data, packet->BitSize, out packetId, out bitLength, out data);
    }

    public static bool TryReadPacketBitStream(CRakNetBitStream* bitStream, out int packetId, out int bitLength, out ReadOnlySpan<byte> data)
    {
        packetId = 0;
        bitLength = 0;
        data = default;

        if (bitStream == null)
        {
            return false;
        }

        return TryReadPacketData(bitStream->Data, bitStream->NumberOfBitsUsed, out packetId, out bitLength, out data);
    }

    public static bool TryReadOutgoingRpcBitStream(int* uniqueId, CRakNetBitStream* bitStream, out int rpcId, out int bitLength, out ReadOnlySpan<byte> data)
    {
        rpcId = 0;
        bitLength = 0;
        data = default;

        if (uniqueId == null || bitStream == null)
        {
            return false;
        }

        return TryReadOutgoingRpcData(*uniqueId, bitStream->Data, bitStream->NumberOfBitsUsed, out rpcId, out bitLength, out data);
    }

    public static bool TryReadOutgoingRpcData(int rpcIdValue, byte* dataPtr, int dataBitLength, out int rpcId, out int bitLength, out ReadOnlySpan<byte> data)
    {
        rpcId = rpcIdValue;
        bitLength = 0;
        data = default;

        if (rpcId < 0 || dataPtr == null || dataBitLength < 0)
        {
            return false;
        }

        int byteLength = GetByteLength(dataBitLength);
        bitLength = dataBitLength;
        data = byteLength == 0 ? ReadOnlySpan<byte>.Empty : new ReadOnlySpan<byte>(dataPtr, byteLength);
        return true;
    }

    public static byte[] CopyPayload(ReadOnlySpan<byte> data)
    {
        return data.ToArray();
    }

    public static int GetByteLength(int bitLength)
    {
        return bitLength <= 0 ? 0 : (bitLength + 7) / 8;
    }

    private static bool TryReadPacketData(byte* dataPtr, int dataBitLength, out int packetId, out int bitLength, out ReadOnlySpan<byte> data)
    {
        packetId = 0;
        bitLength = 0;
        data = default;

        if (dataPtr == null || dataBitLength < 8)
        {
            return false;
        }

        int byteLength = GetByteLength(dataBitLength);
        packetId = dataPtr[0];
        bitLength = dataBitLength;
        data = new ReadOnlySpan<byte>(dataPtr, byteLength);
        return true;
    }
}
