using System.Text;

namespace SFSharp.Abstractions.Interop.RakNet.BitStream;

/// <summary>
/// Managed RakNet-compatible bitstream writer. Builds a <c>byte[]</c> payload whose layout matches
/// the one produced by <c>RakNet::BitStream</c> so plugins can hand the result to
/// <see cref="Network.ISFNetwork.SendPacket"/>, <see cref="Network.ISFNetwork.SendRpc(int, System.ReadOnlySpan{byte}, int, Network.RakNetPacketPriority, Network.RakNetPacketReliability, byte, bool)"/>
/// or <see cref="Network.ISFNetwork.SimulateIncomingPacket"/>.
/// </summary>
/// <remarks>
/// Internal layout tracks a bit cursor, so every write advances by an exact bit count. For
/// byte-aligned writes the output is identical to little-endian raw byte writes, so this type is
/// drop-in interchangeable with <see cref="System.Buffers.Binary.BinaryPrimitives"/> for those
/// cases but also handles compressed ints and unaligned writes that require real bit packing.
/// Not thread-safe: create one instance per packet being built.
/// </remarks>
public ref struct BitStreamWriter
{
    /// <summary>
    /// Single-byte encoding shared by all string writes. Arizona-RP and SA-MP traffic uses
    /// Windows-1251 historically, so the provider is registered here to survive environments that
    /// ship trimmed ICU tables.
    /// </summary>
    private static readonly Encoding _stringEncoding;

    static BitStreamWriter()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _stringEncoding = Encoding.GetEncoding(1251);
    }

    private byte[] _buffer;
    private int _bitsUsed;

    /// <summary>Creates a writer with a 256-byte starting buffer. Grows on demand.</summary>
    public BitStreamWriter() : this(256) { }

    /// <summary>Creates a writer with the requested starting capacity (floored at 16 bytes).</summary>
    /// <param name="initialCapacityBytes">Initial backing array size in bytes.</param>
    public BitStreamWriter(int initialCapacityBytes)
    {
        _buffer = new byte[Math.Max(initialCapacityBytes, 16)];
        _bitsUsed = 0;
    }

    /// <summary>Number of bits already written.</summary>
    public int BitLength => _bitsUsed;

    /// <summary>Number of bytes already written, rounded up to the nearest full byte.</summary>
    public int ByteLength => (_bitsUsed + 7) / 8;

    /// <summary>Returns a view over the written prefix of the backing buffer.</summary>
    /// <remarks>Valid only as long as no further writes happen; subsequent writes may resize the buffer.</remarks>
    public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan(0, ByteLength);

    /// <summary>Copies the written prefix into a fresh <c>byte[]</c>.</summary>
    public byte[] ToArray() => AsSpan().ToArray();

    /// <summary>Writes a single bit (<see langword="true"/> → 1, <see langword="false"/> → 0).</summary>
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

    /// <summary>Writes one <see cref="byte"/> (8 bits, right-aligned).</summary>
    public unsafe void WriteUInt8(byte value)
    {
        WriteBitsInternal(&value, 8, alignRight: true);
    }

    /// <summary>
    /// Mirrors <c>RakNet::BitStream::WriteCompressed&lt;unsigned int&gt;</c>: emits one "is-zero" bit per
    /// higher byte and then the remainder verbatim. The low byte is split as a 4-bit nibble when its
    /// high half is zero. Used by the ID_RPC wrapper for payload bit-length.
    /// </summary>
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

    /// <summary>Writes an <see cref="ushort"/> as 16 little-endian bits.</summary>
    public unsafe void WriteUInt16(ushort value)
    {
        WriteBitsInternal((byte*)&value, 16, alignRight: false);
    }

    /// <summary>Writes a <see cref="short"/> as 16 little-endian bits.</summary>
    public unsafe void WriteInt16(short value)
    {
        WriteBitsInternal((byte*)&value, 16, alignRight: false);
    }

    /// <summary>Writes a <see cref="uint"/> as 32 little-endian bits.</summary>
    public unsafe void WriteUInt32(uint value)
    {
        WriteBitsInternal((byte*)&value, 32, alignRight: false);
    }

    /// <summary>Writes an <see cref="int"/> as 32 little-endian bits.</summary>
    public unsafe void WriteInt32(int value)
    {
        WriteBitsInternal((byte*)&value, 32, alignRight: false);
    }

    /// <summary>Writes a <see cref="float"/> as its IEEE-754 little-endian representation.</summary>
    public unsafe void WriteFloat(float value)
    {
        WriteBitsInternal((byte*)&value, 32, alignRight: false);
    }

    /// <summary>
    /// Writes any unmanaged value type bit-for-bit. Uses the in-memory byte layout (little-endian
    /// on all RakNet-supported platforms). Useful for packed structs like vectors and quaternions.
    /// </summary>
    public unsafe void Write<T>(T value) where T : unmanaged
    {
        WriteBitsInternal((byte*)&value, sizeof(T) * 8, alignRight: false);
    }

    /// <summary>Copies <paramref name="data"/> verbatim into the stream. No-op if empty.</summary>
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

    /// <summary>
    /// Writes <paramref name="value"/> encoded in Windows-1251, padded with zero bytes (or
    /// truncated) to exactly <paramref name="byteLength"/> bytes. Standard SA-MP fixed-string slot.
    /// </summary>
    public void WriteFixedString(string value, int byteLength)
    {
        byte[] encoded = new byte[byteLength];
        if (!string.IsNullOrEmpty(value))
        {
            _stringEncoding.GetBytes(value, 0, Math.Min(value.Length, byteLength), encoded, 0);
        }
        WriteBytes(encoded);
    }

    /// <summary>Writes a length-prefixed Windows-1251 string with an 8-bit length header.</summary>
    public void WriteStringUInt8Length(string value)
    {
        byte[] encoded = string.IsNullOrEmpty(value) ? [] : _stringEncoding.GetBytes(value);
        WriteUInt8((byte)encoded.Length);
        WriteBytes(encoded);
    }

    /// <summary>Writes a length-prefixed Windows-1251 string with a 16-bit length header.</summary>
    public void WriteStringUInt16Length(string value)
    {
        byte[] encoded = string.IsNullOrEmpty(value) ? [] : _stringEncoding.GetBytes(value);
        WriteUInt16((ushort)encoded.Length);
        WriteBytes(encoded);
    }

    /// <summary>Writes a length-prefixed Windows-1251 string with a 32-bit length header.</summary>
    public void WriteStringUInt32Length(string value)
    {
        byte[] encoded = string.IsNullOrEmpty(value) ? [] : _stringEncoding.GetBytes(value);
        WriteUInt32((uint)encoded.Length);
        WriteBytes(encoded);
    }

    /// <summary>Grows the backing buffer so it can fit <paramref name="bitsNeeded"/> additional bits.</summary>
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

    /// <summary>
    /// Core write routine. Fast-paths byte-aligned writes with a plain <c>memcpy</c>-style loop;
    /// otherwise packs the value bit-by-bit starting from the MSB of the current byte.
    /// <paramref name="alignRight"/> pre-shifts single-byte inputs so an 8-bit value lands in the
    /// same bit-order a real RakNet writer would produce when the cursor is not byte-aligned.
    /// </summary>
    private unsafe void WriteBitsInternal(byte* input, int bitCount, bool alignRight)
    {
        EnsureCapacity(bitCount);

        int byteCount = (bitCount + 7) / 8;

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
