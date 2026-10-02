using System.Text;
using SFSharp.Runtime.Interop.RakNet;

namespace SF.Network.Tests;

[Collection(nameof(SampStringDecoderTests))]
public sealed unsafe class SampStringDecoderTests
{
    [Fact]
    public void ReadEncodedStringUsesTheHostDecoderAndAdvancesPastTheString()
    {
        ISampStringDecoder? previous = SampBitStreamReader.NativeStringDecoder;
        SampBitStreamReader.NativeStringDecoder = new FixedDecoder(Cp1251("Привет"), consumedBits: 24);
        try
        {
            byte[] data = [0x00, 0x00, 0x00, 0xAB];
            fixed (byte* ptr = data)
            {
                SampBitStreamReader reader = new(ptr, 0, data.Length * 8);

                Assert.Equal("Привет", reader.ReadEncodedString(64));
                Assert.Equal(24, reader.OffsetBits);
                Assert.Equal(0xAB, reader.ReadUInt8());
            }
        }
        finally
        {
            SampBitStreamReader.NativeStringDecoder = previous;
        }
    }

    private static byte[] Cp1251(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1251).GetBytes(text);
    }

    private sealed class FixedDecoder(byte[] text, int consumedBits) : ISampStringDecoder
    {
        public bool TryDecode(byte* data, int bitLength, ref int bitOffset, Span<byte> output, out int length)
        {
            text.CopyTo(output);
            output[text.Length] = 0;
            length = text.Length;
            bitOffset += consumedBits;
            return true;
        }
    }
}

[CollectionDefinition(nameof(SampStringDecoderTests), DisableParallelization = true)]
public sealed class SampStringDecoderCollection;
