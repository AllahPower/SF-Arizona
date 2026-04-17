using System.Text;

namespace SFSharp.Runtime.Networking;

/// <summary>
/// Managed bitstream writer, mirrors BitStreamReader.
/// Builds a byte[] payload for sending via RakNet.
/// </summary>
public ref struct BitStreamWriter
{
    private static readonly Encoding _stringEncoding;

    static BitStreamWriter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _stringEncoding = Encoding.GetEncoding(1251);
    }

    private byte[] _buffer;
    private int _bitsUsed;

    public BitStreamWriter() : this(256) { }

    public BitStreamWriter(int initialCapacityBytes)
    {
        _buffer = new byte[Math.Max(initialCapacityBytes, 16)];
        _bitsUsed = 0;
    }

    public int BitLength => _bitsUsed;
    public int ByteLength => (_bitsUsed + 7) / 8;

    public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan(0, ByteLength);

    public byte[] ToArray() => AsSpan().ToArray();

    public void WriteBitBool(bool value)
    {
        EnsureCapacity(1);
        if (value)
        {
            int byteIndex = _bitsUsed / 8;
            int bitIndex = 7 - (_bitsUsed & 7);
            _buffer[byteIndex] |= (byte)(1 << bitIndex);
        }
        _bitsUsed++;
    }

    public unsafe void WriteUInt8(byte value)
    {
        WriteBitsInternal(&value, 8, alignRight: true);
    }

    // Mirrors RakNet::BitStream::WriteCompressed<unsigned int>: emits one "is-zero" bit per
    // higher byte, then the remainder verbatim; the low byte is split as a 4-bit nibble when
    // its high half is zero. Used in the ID_RPC wrapper for payload bit-length.
    public unsafe void WriteCompressedUInt32(uint value)
    {
        byte* bytes = (byte*)&value;
        for (int i = sizeof(uint) - 1; i > 0; i--)
        {
            bool isZero = bytes[i] == 0;
            WriteBitBool(isZero);
            if (!isZero)
            {
                WriteBitsInternal(bytes, (i + 1) * 8, alignRight: false);
                return;
            }
        }

        bool lowNibbleOnly = (bytes[0] & 0xF0) == 0;
        WriteBitBool(lowNibbleOnly);
        WriteBitsInternal(bytes, lowNibbleOnly ? 4 : 8, alignRight: true);
    }

    public unsafe void WriteUInt16(ushort value)
    {
        WriteBitsInternal((byte*)&value, 16, alignRight: false);
    }

    public unsafe void WriteInt16(short value)
    {
        WriteBitsInternal((byte*)&value, 16, alignRight: false);
    }

    public unsafe void WriteUInt32(uint value)
    {
        WriteBitsInternal((byte*)&value, 32, alignRight: false);
    }

    public unsafe void WriteInt32(int value)
    {
        WriteBitsInternal((byte*)&value, 32, alignRight: false);
    }

    public unsafe void WriteFloat(float value)
    {
        WriteBitsInternal((byte*)&value, 32, alignRight: false);
    }

    public unsafe void Write<T>(T value) where T : unmanaged
    {
        WriteBitsInternal((byte*)&value, sizeof(T) * 8, alignRight: false);
    }

    public void WriteBytes(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        unsafe
        {
            fixed (byte* ptr = data)
            {
                WriteBitsInternal(ptr, data.Length * 8, alignRight: false);
            }
        }
    }

    public void WriteFixedString(string value, int byteLength)
    {
        byte[] encoded = new byte[byteLength];
        if (!string.IsNullOrEmpty(value))
        {
            _stringEncoding.GetBytes(value, 0, Math.Min(value.Length, byteLength), encoded, 0);
        }
        WriteBytes(encoded);
    }

    public void WriteStringUInt8Length(string value)
    {
        byte[] encoded = string.IsNullOrEmpty(value) ? [] : _stringEncoding.GetBytes(value);
        WriteUInt8((byte)encoded.Length);
        WriteBytes(encoded);
    }

    public void WriteStringUInt16Length(string value)
    {
        byte[] encoded = string.IsNullOrEmpty(value) ? [] : _stringEncoding.GetBytes(value);
        WriteUInt16((ushort)encoded.Length);
        WriteBytes(encoded);
    }

    public void WriteStringUInt32Length(string value)
    {
        byte[] encoded = string.IsNullOrEmpty(value) ? [] : _stringEncoding.GetBytes(value);
        WriteUInt32((uint)encoded.Length);
        WriteBytes(encoded);
    }

    private void EnsureCapacity(int bitsNeeded)
    {
        int totalBitsNeeded = _bitsUsed + bitsNeeded;
        int totalBytesNeeded = (totalBitsNeeded + 7) / 8;
        if (totalBytesNeeded <= _buffer.Length) return;

        int newSize = _buffer.Length;
        while (newSize < totalBytesNeeded) newSize *= 2;

        byte[] newBuffer = new byte[newSize];
        Buffer.BlockCopy(_buffer, 0, newBuffer, 0, ByteLength);
        _buffer = newBuffer;
    }

    private unsafe void WriteBitsInternal(byte* input, int bitCount, bool alignRight)
    {
        EnsureCapacity(bitCount);

        int byteCount = (bitCount + 7) / 8;

        // Fast path: byte-aligned write
        if ((_bitsUsed & 7) == 0 && (bitCount & 7) == 0)
        {
            int byteOffset = _bitsUsed / 8;
            for (int i = 0; i < byteCount; i++)
            {
                _buffer[byteOffset + i] = input[i];
            }
            _bitsUsed += bitCount;
            return;
        }

        // alignRight: the value is right-justified in the last byte (e.g. single byte for uint8)
        // We need to shift it left before writing bits
        byte[] source = new byte[byteCount];
        for (int i = 0; i < byteCount; i++) source[i] = input[i];

        int remainderBits = bitCount & 7;
        if (alignRight && remainderBits != 0)
        {
            source[byteCount - 1] <<= 8 - remainderBits;
        }

        for (int i = 0; i < bitCount; i++)
        {
            int sourceByteIndex = i / 8;
            int sourceBitIndex = 7 - (i & 7);
            int bit = (source[sourceByteIndex] >> sourceBitIndex) & 1;

            int targetBit = _bitsUsed + i;
            int targetByteIndex = targetBit / 8;
            int targetBitIndex = 7 - (targetBit & 7);
            _buffer[targetByteIndex] |= (byte)(bit << targetBitIndex);
        }

        _bitsUsed += bitCount;
    }
}
