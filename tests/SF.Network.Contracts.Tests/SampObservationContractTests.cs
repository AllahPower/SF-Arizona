using SFSharp.Abstractions.Samp;
using SFSharp.Abstractions.Samp.Events;

namespace SF.Network.Contracts.Tests;

public sealed class SampObservationContractTests
{
    [Theory]
    [InlineData(nameof(ISFSampActions.Session), typeof(ISFSampSessionEvents))]
    [InlineData(nameof(ISFSampActions.RemotePlayers), typeof(ISFSampRemotePlayerEvents))]
    [InlineData(nameof(ISFSampActions.Pools), typeof(ISFSampPoolEvents))]
    [InlineData(nameof(ISFSampActions.TextDraws), typeof(ISFSampTextDrawEvents))]
    [InlineData(nameof(ISFSampActions.Pickups), typeof(ISFSampPickupEvents))]
    [InlineData(nameof(ISFSampActions.Spectating), typeof(ISFSampSpectatingEvents))]
    public void ObserverGroupsExposeDisposableSubscriptions(string property, Type contract)
    {
        Assert.Equal(contract, typeof(ISFSampActions).GetProperty(property)?.PropertyType);
        Assert.All(contract.GetMethods(), method => Assert.Equal(typeof(IDisposable), method.ReturnType));
    }

    [Fact]
    public void SyncFramesOwnTheirBytesAndDoNotExposeNativePointers()
    {
        Assert.True(typeof(SFSampSyncPayload).IsValueType);
        Assert.True(typeof(SFSampRemoteSnapshot).IsValueType);
        Assert.Equal(typeof(uint?), typeof(SFSampRemoteSyncEvent).GetProperty(nameof(SFSampRemoteSyncEvent.NativeTimestamp))?.PropertyType);
        Assert.DoesNotContain(typeof(SFSampRemoteSyncEvent).GetProperties(), property => property.PropertyType == typeof(nint));

        byte[] bytes = [1, 2, 3];
        var copy = new SFSampSyncPayload(bytes);
        bytes[0] = 99;
        Assert.Equal((byte)1, copy.ReadByte(0));
    }

    [Fact]
    public void MissingFeatureErrorKeepsStructuredRevisionAndFeature()
    {
        var exception = new SFSampFeatureUnavailableException(SFSampVersion.R1, "Connect");

        Assert.IsAssignableFrom<NotSupportedException>(exception);
        Assert.Equal(SFSampVersion.R1, exception.Version);
        Assert.Equal("Connect", exception.Feature);
    }
}
