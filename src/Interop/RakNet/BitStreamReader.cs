using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

/// <summary>
/// Managed reader over raw RPC bitstream data.
/// Supports arbitrary payload bit offsets so incoming RPC detours can avoid bit realignment.
/// </summary>
public unsafe ref struct BitStreamReader
{
    private static readonly Encoding _stringEncoding;
    private static readonly HuffmanNode _stringCompressorRoot;
    private static readonly delegate* unmanaged[Stdcall]<nint> _getStringCompressorInstance;
    private static readonly delegate* unmanaged[Thiscall]<nint, byte*, int, SampBitStream*, ushort, byte> _stringCompressorDecodeString;

    static BitStreamReader()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _stringEncoding = Encoding.GetEncoding(1251);
        _stringCompressorRoot = BuildHuffmanTree(s_englishCharacterFrequencies);
        _getStringCompressorInstance = (delegate* unmanaged[Stdcall]<nint>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.SampStringCompressor.Instance);
        _stringCompressorDecodeString = (delegate* unmanaged[Thiscall]<nint, byte*, int, SampBitStream*, ushort, byte>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.SampStringCompressor.DecodeString);
    }

    private readonly byte* _data;
    private readonly int _startBitOffset;
    private readonly int _endBitOffset;
    private int _offsetBits;

    public BitStreamReader(byte* data, int startBitOffset, int lengthBits)
    {
        _data = data;
        _startBitOffset = startBitOffset;
        _endBitOffset = startBitOffset + lengthBits;
        _offsetBits = startBitOffset;
    }

    public int RemainingBits => _endBitOffset - _offsetBits;
    public int OffsetBits => _offsetBits - _startBitOffset;
    public int LengthBits => _endBitOffset - _startBitOffset;
    public int LengthBytes => (LengthBits + 7) / 8;

    public static bool RakNetBitStreamDecodeString(ReadOnlySpan<byte> encodedPayload, int maxCharsToWrite, out string text)
    {
        if (maxCharsToWrite <= 0)
        {
            text = string.Empty;
            return true;
        }

        if (encodedPayload.IsEmpty)
        {
            text = string.Empty;
            return false;
        }

        if (TryDecodeManaged(encodedPayload, maxCharsToWrite, out text) && !string.IsNullOrEmpty(text))
        {
            return true;
        }

        return TryDecodeNative(encodedPayload, maxCharsToWrite, out text);
    }

    public bool ReadBitBool()
    {
        EnsureBits(1);
        int sourceByteIndex = _offsetBits / 8;
        int sourceBitIndex = 7 - (_offsetBits & 7);
        bool value = ((_data[sourceByteIndex] >> sourceBitIndex) & 1) != 0;
        _offsetBits++;
        return value;
    }

    public bool ReadBool8()
    {
        return ReadUInt8() != 0;
    }


    public byte ReadUInt8()
    {
        byte value = 0;
        ReadBitsInternal(&value, 8, alignRight: true);
        return value;
    }

    public ushort ReadUInt16()
    {
        ushort value = 0;
        ReadBitsInternal((byte*)&value, 16, alignRight: false);
        return value;
    }

    public short ReadInt16()
    {
        short value = 0;
        ReadBitsInternal((byte*)&value, 16, alignRight: false);
        return value;
    }

    public uint ReadUInt32()
    {
        uint value = 0;
        ReadBitsInternal((byte*)&value, 32, alignRight: false);
        return value;
    }

    public int ReadInt32()
    {
        int value = 0;
        ReadBitsInternal((byte*)&value, 32, alignRight: false);
        return value;
    }

    public float ReadFloat()
    {
        float value = 0;
        ReadBitsInternal((byte*)&value, 32, alignRight: false);
        return value;
    }

    public bool ReadBool32()
    {
        return ReadInt32() != 0;
    }

    public T Read<T>() where T : unmanaged
    {
        T value = default;
        ReadBitsInternal((byte*)&value, sizeof(T) * 8, alignRight: false);
        return value;
    }

    public string ReadFixedString(int byteLength)
    {
        if (byteLength <= 0)
        {
            return string.Empty;
        }

        byte[] bytes = new byte[byteLength];
        fixed (byte* bytesPtr = bytes)
        {
            ReadBitsInternal(bytesPtr, byteLength * 8, alignRight: false);
        }

        ReadOnlySpan<byte> span = bytes;
        int nullIndex = span.IndexOf((byte)0);
        if (nullIndex >= 0)
        {
            span = span[..nullIndex];
        }

        return _stringEncoding.GetString(span);
    }

    public string ReadStringUInt8Length()
    {
        int length = ReadUInt8();
        return ReadFixedString(length);
    }

    public string ReadStringUInt16Length()
    {
        int length = ReadUInt16();
        return ReadFixedString(length);
    }

    public string ReadStringUInt32Length()
    {
        int length = (int)ReadUInt32();
        return ReadFixedString(length);
    }

    public ushort ReadCompressedUInt16()
    {
        byte[] bytes = new byte[2];
        int currentByte = 1;
        while (currentByte > 0)
        {
            if (ReadBitBool())
            {
                bytes[currentByte] = 0;
                currentByte--;
                continue;
            }

            int bitsToRead = (currentByte + 1) * 8;
            byte[] partialBytes = ReadUnalignedBits(bitsToRead, alignRight: true);
            Array.Copy(partialBytes, bytes, partialBytes.Length);
            return (ushort)(bytes[0] | (bytes[1] << 8));
        }

        bool halfByteMarker = ReadBitBool();
        int lowBitsToRead = halfByteMarker ? 4 : 8;
        byte[] lowPart = ReadUnalignedBits(lowBitsToRead, alignRight: true);
        bytes[0] = lowPart[0];
        return (ushort)(bytes[0] | (bytes[1] << 8));
    }

    public string ReadEncodedString(int maxCharsToWrite)
    {
        if (maxCharsToWrite <= 1)
        {
            return string.Empty;
        }

        if (TryReadEncodedStringNative(maxCharsToWrite, out string nativeText))
        {
            return nativeText;
        }

        int sizeInBits = ReadCompressedUInt16();
        if (sizeInBits <= 0)
        {
            return string.Empty;
        }

        byte[] output = new byte[maxCharsToWrite - 1];
        HuffmanNode currentNode = _stringCompressorRoot;
        int outputWriteIndex = 0;
        while (sizeInBits > 0)
        {
            bool bit = ReadBitBool();
            sizeInBits--;
            currentNode = bit ? currentNode.Right! : currentNode.Left!;
            if (!currentNode.IsLeaf)
            {
                continue;
            }

            if (outputWriteIndex == output.Length)
            {
                SkipBits(sizeInBits);
                break;
            }

            output[outputWriteIndex++] = currentNode.Value;
            currentNode = _stringCompressorRoot;
        }

        return outputWriteIndex > 0 ? _stringEncoding.GetString(output, 0, outputWriteIndex).TrimEnd('\0') : string.Empty;
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (count <= 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        byte[] bytes = new byte[count];
        fixed (byte* bytesPtr = bytes)
        {
            ReadBitsInternal(bytesPtr, count * 8, alignRight: false);
        }

        return bytes;
    }

    public byte[] ReadRemainingBytes()
    {
        int remainingBytes = (RemainingBits + 7) / 8;
        byte[] bytes = new byte[remainingBytes];
        if (remainingBytes == 0)
        {
            return bytes;
        }

        fixed (byte* bytesPtr = bytes)
        {
            ReadBitsInternal(bytesPtr, RemainingBits, alignRight: false);
        }

        return bytes;
    }

    public void SkipBits(int count)
    {
        EnsureBits(count);
        _offsetBits += count;
    }

    public void SkipBytes(int count) => SkipBits(count * 8);

    public void Reset() => _offsetBits = _startBitOffset;

    private byte[] ReadUnalignedBits(int bitCount, bool alignRight)
    {
        if (bitCount <= 0)
        {
            return Array.Empty<byte>();
        }

        byte[] bytes = new byte[(bitCount + 7) / 8];
        fixed (byte* bytesPtr = bytes)
        {
            ReadBitsInternal(bytesPtr, bitCount, alignRight);
        }

        return bytes;
    }

    private static bool TryDecodeManaged(ReadOnlySpan<byte> encodedPayload, int maxCharsToWrite, out string text)
    {
        RakNetBitReader reader = new(encodedPayload);
        if (!reader.TryReadCompressedUInt16(out ushort stringBitLength))
        {
            text = string.Empty;
            return false;
        }

        if (reader.RemainingBits < stringBitLength)
        {
            text = string.Empty;
            return false;
        }

        int maxOutputLength = Math.Max(0, maxCharsToWrite - 1);
        byte[] output = new byte[maxOutputLength];
        int bytesWritten = DecodeArray(ref reader, stringBitLength, output);
        text = bytesWritten > 0 ? _stringEncoding.GetString(output, 0, bytesWritten).TrimEnd('\0') : string.Empty;
        return true;
    }

    private static bool TryDecodeNative(ReadOnlySpan<byte> encodedPayload, int maxCharsToWrite, out string text)
    {
        nint instance = _getStringCompressorInstance();
        if (instance == 0)
        {
            text = string.Empty;
            return false;
        }

        byte[] output = new byte[maxCharsToWrite];
        fixed (byte* payloadPtr = encodedPayload)
        fixed (byte* outputPtr = output)
        {
            SampBitStream bitStream = new()
            {
                NumberOfBitsAllocated = encodedPayload.Length * 8,
                NumberOfBitsUsed = encodedPayload.Length * 8,
                ReadOffset = 0,
                Data = payloadPtr
            };

            byte ok = _stringCompressorDecodeString(instance, outputPtr, maxCharsToWrite, &bitStream, 0);
            if (ok == 0)
            {
                text = string.Empty;
                return false;
            }
        }

        int nullIndex = Array.IndexOf(output, (byte)0);
        if (nullIndex < 0)
        {
            nullIndex = output.Length;
        }

        text = _stringEncoding.GetString(output, 0, nullIndex);
        return true;
    }

    private bool TryReadEncodedStringNative(int maxCharsToWrite, out string text)
    {
        nint instance = _getStringCompressorInstance();
        if (instance == 0)
        {
            text = string.Empty;
            return false;
        }

        int nativeBufferChars = Math.Max(maxCharsToWrite, 0x1000);
        byte[] output = new byte[nativeBufferChars];
        SampBitStream bitStream = new()
        {
            NumberOfBitsAllocated = _endBitOffset,
            NumberOfBitsUsed = _endBitOffset,
            ReadOffset = _offsetBits,
            Data = _data
        };

        fixed (byte* outputPtr = output)
        {
            byte ok = _stringCompressorDecodeString(instance, outputPtr, nativeBufferChars, &bitStream, 0);
            if (ok == 0)
            {
                text = string.Empty;
                return false;
            }
        }

        _offsetBits = bitStream.ReadOffset;

        int nullIndex = Array.IndexOf(output, (byte)0);
        if (nullIndex < 0)
        {
            nullIndex = output.Length;
        }

        text = _stringEncoding.GetString(output, 0, nullIndex);
        return true;
    }

    private static int DecodeArray(ref RakNetBitReader reader, int sizeInBits, byte[] output)
    {
        HuffmanNode currentNode = _stringCompressorRoot;
        int outputWriteIndex = 0;
        while (sizeInBits > 0)
        {
            if (outputWriteIndex == output.Length)
            {
                reader.SkipBits(sizeInBits);
                break;
            }

            if (!reader.TryReadBit(out bool bit))
            {
                break;
            }

            sizeInBits--;
            currentNode = bit ? currentNode.Right! : currentNode.Left!;
            if (currentNode.IsLeaf)
            {
                output[outputWriteIndex++] = currentNode.Value;
                currentNode = _stringCompressorRoot;
            }
        }

        return outputWriteIndex;
    }

    private static HuffmanNode BuildHuffmanTree(uint[] frequencyTable)
    {
        List<HuffmanNode> nodes = new(256);
        for (int i = 0; i < 256; i++)
        {
            uint weight = frequencyTable[i] == 0 ? 1u : frequencyTable[i];
            InsertSorted(nodes, new HuffmanNode((byte)i, weight));
        }

        while (true)
        {
            HuffmanNode lesser = nodes[0];
            HuffmanNode greater = nodes[1];
            nodes.RemoveRange(0, 2);

            HuffmanNode merged = new(lesser, greater);
            if (nodes.Count == 0)
            {
                return merged;
            }

            InsertSorted(nodes, merged);
        }
    }

    private static void InsertSorted(List<HuffmanNode> nodes, HuffmanNode node)
    {
        int index = 0;
        while (index < nodes.Count && nodes[index].Weight < node.Weight)
        {
            index++;
        }

        nodes.Insert(index, node);
    }

    private void EnsureBits(int count)
    {
        if (_offsetBits + count > _endBitOffset)
        {
            throw new InvalidOperationException($"BitStream underflow: need {count} bits at offset {OffsetBits}, total {LengthBits}");
        }
    }

    private void ReadBitsInternal(byte* output, int bitCount, bool alignRight)
    {
        EnsureBits(bitCount);

        int byteCount = (bitCount + 7) / 8;
        for (int i = 0; i < byteCount; i++)
        {
            output[i] = 0;
        }

        if ((_offsetBits & 7) == 0 && (bitCount & 7) == 0)
        {
            int byteOffset = _offsetBits / 8;
            Buffer.MemoryCopy(_data + byteOffset, output, byteCount, byteCount);
            _offsetBits += bitCount;
            return;
        }

        for (int i = 0; i < bitCount; i++)
        {
            int sourceBit = _offsetBits + i;
            int sourceByteIndex = sourceBit / 8;
            int sourceBitIndex = 7 - (sourceBit & 7);
            int bit = (_data[sourceByteIndex] >> sourceBitIndex) & 1;

            int targetByteIndex = i / 8;
            int targetBitIndex = 7 - (i & 7);
            output[targetByteIndex] = (byte)(output[targetByteIndex] | (bit << targetBitIndex));
        }

        _offsetBits += bitCount;

        int remainderBits = bitCount & 7;
        if (alignRight && remainderBits != 0)
        {
            output[byteCount - 1] >>= 8 - remainderBits;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SampBitStream
    {
        public int NumberOfBitsAllocated;
        public int NumberOfBitsUsed;
        public int ReadOffset;
        public byte* Data;
    }

    private sealed class HuffmanNode
    {
        public HuffmanNode(byte value, uint weight)
        {
            Value = value;
            Weight = weight;
        }

        public HuffmanNode(HuffmanNode left, HuffmanNode right)
        {
            Left = left;
            Right = right;
            Weight = left.Weight + right.Weight;
        }

        public byte Value { get; }
        public uint Weight { get; }
        public HuffmanNode? Left { get; }
        public HuffmanNode? Right { get; }
        public bool IsLeaf => Left is null && Right is null;
    }

    private ref struct RakNetBitReader
    {
        private readonly ReadOnlySpan<byte> _data;
        private readonly int _lengthBits;
        private int _offsetBits;

        public RakNetBitReader(ReadOnlySpan<byte> data)
        {
            _data = data;
            _lengthBits = data.Length * 8;
            _offsetBits = 0;
        }

        public int RemainingBits => _lengthBits - _offsetBits;

        public bool TryReadCompressedUInt16(out ushort value)
        {
            byte[] bytes = new byte[2];
            int currentByte = 1;
            while (currentByte > 0)
            {
                if (!TryReadBit(out bool marker))
                {
                    value = 0;
                    return false;
                }

                if (marker)
                {
                    bytes[currentByte] = 0;
                    currentByte--;
                }
                else
                {
                    byte[] partialBytes = new byte[currentByte + 1];
                    if (!TryReadBits(partialBytes, (currentByte + 1) * 8, true))
                    {
                        value = 0;
                        return false;
                    }

                    Array.Copy(partialBytes, bytes, partialBytes.Length);
                    value = (ushort)(bytes[0] | (bytes[1] << 8));
                    return true;
                }
            }

            if (!TryReadBit(out bool halfByteMarker))
            {
                value = 0;
                return false;
            }

            byte[] lowPart = new byte[1];
            int bitsToRead = halfByteMarker ? 4 : 8;
            if (!TryReadBits(lowPart, bitsToRead, true))
            {
                value = 0;
                return false;
            }

            bytes[0] = lowPart[0];
            value = (ushort)(bytes[0] | (bytes[1] << 8));
            return true;
        }

        public bool TryReadBit(out bool value)
        {
            if (_offsetBits >= _lengthBits)
            {
                value = false;
                return false;
            }

            int sourceByteIndex = _offsetBits / 8;
            int sourceBitIndex = 7 - (_offsetBits & 7);
            value = ((_data[sourceByteIndex] >> sourceBitIndex) & 1) != 0;
            _offsetBits++;
            return true;
        }

        public void SkipBits(int count)
        {
            _offsetBits = Math.Min(_lengthBits, _offsetBits + count);
        }

        private bool TryReadBits(byte[] output, int bitCount, bool alignRight)
        {
            if (bitCount <= 0)
            {
                Array.Clear(output);
                return true;
            }

            if (RemainingBits < bitCount)
            {
                Array.Clear(output);
                return false;
            }

            Array.Clear(output);
            for (int i = 0; i < bitCount; i++)
            {
                int sourceBit = _offsetBits + i;
                int sourceByteIndex = sourceBit / 8;
                int sourceBitIndex = 7 - (sourceBit & 7);
                int bit = (_data[sourceByteIndex] >> sourceBitIndex) & 1;

                int targetByteIndex = i / 8;
                int targetBitIndex = 7 - (i & 7);
                output[targetByteIndex] = (byte)(output[targetByteIndex] | (bit << targetBitIndex));
            }

            _offsetBits += bitCount;

            int remainderBits = bitCount & 7;
            if (alignRight && remainderBits != 0)
            {
                output[(bitCount + 7) / 8 - 1] >>= 8 - remainderBits;
            }

            return true;
        }
    }

    private static readonly uint[] s_englishCharacterFrequencies = new uint[]
    {
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 722u, 0u, 0u, 2u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        11084u, 58u, 63u, 1u, 0u, 31u, 0u, 317u, 64u, 64u, 44u, 0u, 695u, 62u, 980u, 266u,
        69u, 67u, 56u, 7u, 73u, 3u, 14u, 2u, 69u, 1u, 167u, 9u, 1u, 2u, 25u, 94u,
        0u, 195u, 139u, 34u, 96u, 48u, 103u, 56u, 125u, 653u, 21u, 5u, 23u, 64u, 85u, 44u,
        34u, 7u, 92u, 76u, 147u, 12u, 14u, 57u, 15u, 39u, 15u, 1u, 1u, 1u, 2u, 3u,
        0u, 3611u, 845u, 1077u, 1884u, 5870u, 841u, 1057u, 2501u, 3212u, 164u, 531u, 2019u, 1330u, 3056u, 4037u,
        848u, 47u, 2586u, 2919u, 4771u, 1707u, 535u, 1106u, 152u, 1243u, 100u, 0u, 2u, 0u, 10u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
        0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
    };
}
