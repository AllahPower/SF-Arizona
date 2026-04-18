using System.Text;

namespace SFSharp.Abstractions.Interop.RakNet.BitStream;

/// <summary>
/// Managed RakNet-compatible bitstream reader. Mirrors <see cref="BitStreamWriter"/> for the
/// decoding side so plugins can parse payloads they receive through the typed event facades, from
/// <see cref="Network.ISFNetwork.SimulateIncomingPacket"/> echoes, or from any other raw byte span.
/// </summary>
/// <remarks>
/// Supports arbitrary start bit offsets so callers whose payload is not byte-aligned (e.g. raw RPC
/// containers) can point the reader at the correct starting bit without pre-shifting the data.
/// The reader intentionally ships only the primitives and fixed-length Windows-1251 strings -
/// RakNet's Huffman-encoded string path depends on the SA-MP <c>StringCompressor</c> singleton
/// inside <c>samp.dll</c> and lives in the host runtime. Not thread-safe.
/// </remarks>
public unsafe ref struct BitStreamReader
{
    /// <summary>Windows-1251 decoder shared with <see cref="BitStreamWriter"/>.</summary>
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

    /// <summary>
    /// Wraps an unmanaged byte buffer with an explicit bit window <c>[startBitOffset, startBitOffset + lengthBits)</c>.
    /// The caller is responsible for keeping the memory pinned / alive for the reader's lifetime.
    /// </summary>
    /// <param name="data">Pointer to the first byte of the backing storage.</param>
    /// <param name="startBitOffset">Bit index inside <paramref name="data"/> where reading begins.</param>
    /// <param name="lengthBits">Length of the readable window in bits.</param>
    public BitStreamReader(byte* data, int startBitOffset, int lengthBits)
    {
        _data = data;
        _startBitOffset = startBitOffset;
        _endBitOffset = startBitOffset + lengthBits;
        _offsetBits = startBitOffset;
    }

    /// <summary>Convenience constructor: wraps a managed span starting at bit 0.</summary>
    /// <remarks>
    /// The caller must keep <paramref name="data"/> pinned for the reader's lifetime; typically the
    /// span itself is a <c>stackalloc</c> or obtained from a <c>fixed</c> pointer.
    /// </remarks>
    public BitStreamReader(ReadOnlySpan<byte> data)
        : this(GetPointer(data), 0, data.Length * 8)
    {
    }

    /// <summary>Bits available between the current cursor and the end of the window.</summary>
    public int RemainingBits => _endBitOffset - _offsetBits;

    /// <summary>Bits already consumed since <see cref="Reset"/> / construction.</summary>
    public int OffsetBits => _offsetBits - _startBitOffset;

    /// <summary>Total bit length of the window.</summary>
    public int LengthBits => _endBitOffset - _startBitOffset;

    /// <summary>Total byte length of the window, rounded up.</summary>
    public int LengthBytes => (LengthBits + 7) / 8;

    /// <summary>Reads a single bit as a <see cref="bool"/>.</summary>
    public bool ReadBitBool()
    {
        EnsureBits(1);
        int sourceByteIndex = _offsetBits / 8;
        int sourceBitIndex = 7 - (_offsetBits & 7);
        bool value = ((_data[sourceByteIndex] >> sourceBitIndex) & 1) != 0;
        _offsetBits++;
        return value;
    }

    /// <summary>Reads a full byte as a boolean (any non-zero value is <see langword="true"/>).</summary>
    public bool ReadBool8() => ReadUInt8() != 0;

    /// <summary>Reads a full <see cref="int"/> as a boolean (any non-zero value is <see langword="true"/>).</summary>
    public bool ReadBool32() => ReadInt32() != 0;

    /// <summary>Reads an unsigned 8-bit integer.</summary>
    public byte ReadUInt8()
    {
        byte value = 0;
        ReadBitsInternal(&value, 8, alignRight: true);
        return value;
    }

    /// <summary>Reads an unsigned 16-bit little-endian integer.</summary>
    public ushort ReadUInt16()
    {
        ushort value = 0;
        ReadBitsInternal((byte*)&value, 16, alignRight: false);
        return value;
    }

    /// <summary>Reads a signed 16-bit little-endian integer.</summary>
    public short ReadInt16()
    {
        short value = 0;
        ReadBitsInternal((byte*)&value, 16, alignRight: false);
        return value;
    }

    /// <summary>Reads an unsigned 32-bit little-endian integer.</summary>
    public uint ReadUInt32()
    {
        uint value = 0;
        ReadBitsInternal((byte*)&value, 32, alignRight: false);
        return value;
    }

    /// <summary>Reads a signed 32-bit little-endian integer.</summary>
    public int ReadInt32()
    {
        int value = 0;
        ReadBitsInternal((byte*)&value, 32, alignRight: false);
        return value;
    }

    /// <summary>Reads an IEEE-754 little-endian single-precision float.</summary>
    public float ReadFloat()
    {
        float value = 0;
        ReadBitsInternal((byte*)&value, 32, alignRight: false);
        return value;
    }

    /// <summary>
    /// Reads an arbitrary unmanaged value type bit-for-bit. Interprets the bytes as the in-memory
    /// layout (little-endian on all RakNet-supported platforms).
    /// </summary>
    public T Read<T>() where T : unmanaged
    {
        T value = default;
        ReadBitsInternal((byte*)&value, sizeof(T) * 8, alignRight: false);
        return value;
    }

    /// <summary>
    /// Reads <paramref name="byteLength"/> bytes, decodes them as Windows-1251 and trims at the
    /// first NUL. Mirrors <see cref="BitStreamWriter.WriteFixedString"/>.
    /// </summary>
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

    /// <summary>Reads an 8-bit length header followed by that many Windows-1251 bytes.</summary>
    public string ReadStringUInt8Length()
    {
        int length = ReadUInt8();
        return ReadFixedString(length);
    }

    /// <summary>Reads a 16-bit length header followed by that many Windows-1251 bytes.</summary>
    public string ReadStringUInt16Length()
    {
        int length = ReadUInt16();
        return ReadFixedString(length);
    }

    /// <summary>Reads a 32-bit length header followed by that many Windows-1251 bytes.</summary>
    public string ReadStringUInt32Length()
    {
        int length = (int)ReadUInt32();
        return ReadFixedString(length);
    }

    /// <summary>
    /// Mirrors <c>RakNet::BitStream::ReadCompressed&lt;unsigned short&gt;</c>. Emits one "is-zero"
    /// marker bit per high byte, then the remainder verbatim; the low byte may be split into a
    /// 4-bit nibble if its upper half was zero when written.
    /// </summary>
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

    /// <summary>Reads <paramref name="count"/> raw bytes and returns them as a fresh buffer.</summary>
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

    /// <summary>Copies every remaining byte in the window into a new array and advances to the end.</summary>
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

    /// <summary>Advances the cursor by <paramref name="count"/> bits without reading anything.</summary>
    public void SkipBits(int count)
    {
        EnsureBits(count);
        _offsetBits += count;
    }

    /// <summary>Advances the cursor by <paramref name="count"/> bytes (convenience wrapper).</summary>
    public void SkipBytes(int count) => SkipBits(count * 8);

    /// <summary>Moves the cursor back to the window start so the payload can be re-parsed.</summary>
    public void Reset() => _offsetBits = _startBitOffset;

    /// <summary>
    /// Pin-helper for <see cref="BitStreamReader(ReadOnlySpan{byte})"/>. Returns a raw pointer to
    /// the span's first element; the span itself must stay alive for the reader's lifetime.
    /// </summary>
    private static byte* GetPointer(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return null;
        fixed (byte* ptr = data) return ptr;
    }

    /// <summary>Reads a bit-unaligned run into a fresh byte array. Used by the compressed-int path.</summary>
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

    /// <summary>Throws when the remaining window is smaller than the request.</summary>
    private void EnsureBits(int count)
    {
        if (_offsetBits + count > _endBitOffset)
        {
            throw new InvalidOperationException(
                $"BitStream underflow: need {count} bits at offset {OffsetBits}, total {LengthBits}");
        }
    }

    /// <summary>
    /// Core read routine. Fast-paths byte-aligned reads with a <c>memcpy</c>; otherwise unpacks bit
    /// by bit. <paramref name="alignRight"/> post-shifts the last partial byte so a read of e.g. 4
    /// bits lands in the low nibble of a byte-sized target, matching <c>RakNet::BitStream::ReadBits</c>.
    /// </summary>
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
