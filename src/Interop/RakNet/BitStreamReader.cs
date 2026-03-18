using System.Runtime.CompilerServices;
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

    static BitStreamReader()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _stringEncoding = Encoding.GetEncoding(1251);
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

    public bool ReadBool()
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
}
