using System.Buffers.Binary;
using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Abstractions.Network;

/// <summary>The ID_TIMESTAMP prefix and inner packet ID in the SA-MP R3 wire format.</summary>
/// <remarks>This is a decoded wire value, not a native ABI structure. It does not validate the inner body.</remarks>
public readonly record struct RakNetTimestampEnvelope(uint Timestamp, byte PacketId)
{
    /// <summary>Bytes before the inner packet ID: one ID_TIMESTAMP byte and a little-endian uint32.</summary>
    public const int PrefixByteLength = 1 + sizeof(uint);

    /// <summary>Minimum complete header size, including the inner packet ID.</summary>
    public const int HeaderByteLength = PrefixByteLength + 1;

    /// <summary>Reads a complete header within the valid bit window without copying or allocating.</summary>
    public static bool TryRead(ReadOnlySpan<byte> data, int dataBitLength, out RakNetTimestampEnvelope envelope)
    {
        envelope = default;
        if (data.Length < HeaderByteLength
            || dataBitLength < HeaderByteLength * 8
            || dataBitLength > (long)data.Length * 8
            || data[0] != (byte)EPacketId.Timestamp)
        {
            return false;
        }

        envelope = new RakNetTimestampEnvelope(
            BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(1, sizeof(uint))),
            data[PrefixByteLength]);
        return true;
    }
}
