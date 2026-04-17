using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct RakNetPlayerId
{
    public uint BinaryAddress;
    public ushort Port;
    private ushort _padding;
}

internal unsafe class IncomingRpcPacketHook : NativeHook<nint, bool, IncomingRpcPacketHook.IncomingRpcPacketNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate bool IncomingRpcPacketNative(IntPtr thisPtr, byte* data, int length, RakNetPlayerId playerId);

    private const byte IdTimestamp = 40;
    private static IncomingRpcPacketHook? _instance;

    /// <summary>
    /// The RakPeer instance pointer, captured from HandleRpcPacket's thisPtr.
    /// Used by SFNetwork.SimulateIncomingPacket to write to the internal packet queue.
    /// </summary>
    internal static nint RakPeerInstance { get; private set; }

    /// <summary>
    /// The server's RakNet PlayerID, captured from the first incoming RPC.
    /// Required for SimulateIncomingPacket so the synthetic packet is dispatched
    /// as server-originated rather than dropped by downstream filters.
    /// </summary>
    internal static RakNetPlayerId ServerPlayerId { get; private set; }

    internal static bool HasServerPlayerId { get; private set; }

    public IncomingRpcPacketHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RpcRuntime.HandleRpcPacket), new IncomingRpcPacketNative(HookProc));
    }

    private static unsafe bool HookProc(IntPtr thisPtr, byte* data, int length, RakNetPlayerId playerId)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        RakPeerInstance = thisPtr;
        ServerPlayerId = playerId;
        HasServerPlayerId = true;

        if (TryExtractRpc(data, length, out int rpcId, out int payloadBitOffset, out int payloadBitLength))
        {
            if (SFBootstrap.IncomingRpcFilters.HasFilters &&
                SFBootstrap.IncomingRpcFilters.ShouldCancel(rpcId, data, length * 8))
            {
                return false;
            }

            if (SFBootstrap.RpcHandlers.HasSubscribers(rpcId))
            {
                byte[] packet = new byte[length];
                fixed (byte* dst = packet)
                {
                    Buffer.MemoryCopy(data, dst, length, length);
                }

                SFBootstrap.EnqueueIncomingRpc(rpcId, packet, payloadBitOffset, payloadBitLength);
            }
        }

        // Call original via MinHook trampoline - no SuppressHook needed
        return _instance.OriginalFunction(thisPtr, data, length, playerId);
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

    private static unsafe bool TryExtractRpc(byte* data, int length, out int rpcId, out int payloadBitOffset, out int payloadBitLength)
    {
        rpcId = 0;
        payloadBitOffset = 0;
        payloadBitLength = 0;

        if (data is null || length <= 1)
        {
            return false;
        }

        int totalBits = length * 8;
        int bitOffset = 8;
        if (data[0] == IdTimestamp)
        {
            bitOffset += 8 * (sizeof(uint) + sizeof(byte));
        }

        if (!TryReadByte(data, totalBits, ref bitOffset, out byte rpcByte))
        {
            return false;
        }

        rpcId = rpcByte;
        bool hasConsumers = SFBootstrap.RpcHandlers.HasSubscribers(rpcId) || SFBootstrap.IncomingRpcFilters.HasFilters;
        if (!hasConsumers)
        {
            return false;
        }

        if (!TryReadCompressedUInt32(data, totalBits, ref bitOffset, out uint bitLength))
        {
            return false;
        }

        if (bitLength > int.MaxValue || bitOffset + (int)bitLength > totalBits)
        {
            return false;
        }

        payloadBitOffset = bitOffset;
        payloadBitLength = (int)bitLength;
        return true;
    }

    private static unsafe bool TryReadByte(byte* data, int totalBits, ref int bitOffset, out byte value)
    {
        byte tempByte = 0;
        if (!TryReadBits(data, totalBits, ref bitOffset, &tempByte, 8, alignRight: true))
        {
            value = 0;
            return false;
        }

        value = tempByte;
        return true;
    }

    private static unsafe bool TryReadCompressedUInt32(byte* data, int totalBits, ref int bitOffset, out uint value)
    {
        value = 0;
        uint buffer = 0;
        byte* bufferPtr = (byte*)&buffer;
        int currentByte = sizeof(uint) - 1;

        while (currentByte > 0)
        {
            if (!TryReadBit(data, totalBits, ref bitOffset, out bool isZeroByte))
            {
                return false;
            }

            if (isZeroByte)
            {
                bufferPtr[currentByte] = 0;
                currentByte--;
                continue;
            }

            return TryReadBits(data, totalBits, ref bitOffset, bufferPtr, (currentByte + 1) << 3, alignRight: true, out value);
        }

        if (!TryReadBit(data, totalBits, ref bitOffset, out bool highNibbleZero))
        {
            return false;
        }

        if (highNibbleZero)
        {
            byte tempNibble = 0;
            if (!TryReadBits(data, totalBits, ref bitOffset, &tempNibble, 4, alignRight: true))
            {
                return false;
            }

            bufferPtr[0] = tempNibble;
            value = buffer;
            return true;
        }

        if (!TryReadBits(data, totalBits, ref bitOffset, bufferPtr, 8, alignRight: true))
        {
            return false;
        }

        value = buffer;
        return true;
    }

    private static unsafe bool TryReadBits(byte* data, int totalBits, ref int bitOffset, byte* output, int bitCount, bool alignRight)
    {
        if (bitOffset + bitCount > totalBits)
        {
            return false;
        }

        int byteCount = (bitCount + 7) / 8;
        for (int i = 0; i < byteCount; i++)
        {
            output[i] = 0;
        }

        for (int i = 0; i < bitCount; i++)
        {
            int sourceBit = bitOffset + i;
            int sourceByteIndex = sourceBit / 8;
            int sourceBitIndex = 7 - (sourceBit % 8);
            int bit = (data[sourceByteIndex] >> sourceBitIndex) & 1;

            int targetByteIndex = i / 8;
            int targetBitIndex = 7 - (i % 8);
            output[targetByteIndex] = (byte)(output[targetByteIndex] | (bit << targetBitIndex));
        }

        bitOffset += bitCount;

        int remainderBits = bitCount & 7;
        if (alignRight && remainderBits != 0)
        {
            output[byteCount - 1] >>= 8 - remainderBits;
        }

        return true;
    }

    private static unsafe bool TryReadBits(byte* data, int totalBits, ref int bitOffset, byte* output, int bitCount, bool alignRight, out uint value)
    {
        value = 0;
        if (!TryReadBits(data, totalBits, ref bitOffset, output, bitCount, alignRight))
        {
            return false;
        }

        value = Unsafe.ReadUnaligned<uint>(output);
        return true;
    }

    private static unsafe bool TryReadBit(byte* data, int totalBits, ref int bitOffset, out bool value)
    {
        value = false;
        if (bitOffset >= totalBits)
        {
            return false;
        }

        int sourceByteIndex = bitOffset / 8;
        int sourceBitIndex = 7 - (bitOffset % 8);
        value = ((data[sourceByteIndex] >> sourceBitIndex) & 1) != 0;
        bitOffset++;
        return true;
    }
}
