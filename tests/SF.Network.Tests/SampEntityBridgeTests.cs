using SFSharp.Runtime.Samp;

namespace SF.Network.Tests;

public sealed class SampEntityBridgeTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(999, true)]
    [InlineData(1000, false)]
    [InlineData(65535, false)]
    public void RejectsInvalidIdsInsteadOfWrapping(int value, bool expected)
    {
        Assert.Equal(expected, SFSampEntities.Accept(value, 1000, out ushort id));
        Assert.Equal(expected ? (ushort)value : ushort.MaxValue, id);
    }
}
