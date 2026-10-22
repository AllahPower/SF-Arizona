using SFSharp.Abstractions.Samp;
using SFSharp.Runtime.Interop.Offsets;

namespace SF.Network.Tests;

public sealed class SampObservationOffsetTests
{
    public static IEnumerable<object[]> AvailableBindings()
    {
        foreach (SFSampVersion version in new[] { SFSampVersion.R1, SFSampVersion.R3_1, SFSampVersion.R5_1 })
            foreach (SampObservationPoint point in Enum.GetValues<SampObservationPoint>())
                if (version != SFSampVersion.R1 || point != SampObservationPoint.Connect)
                    yield return [version, (int)point];
    }

    [Theory]
    [MemberData(nameof(AvailableBindings))]
    public void EverySupportedObservationHasAnExplicitRva(SFSampVersion version, int point)
    {
        int address = SampObservationOffsets.Resolve(version, (SampObservationPoint)point);

        Assert.InRange(address, 0x1000, 0x400000);
    }

    [Fact]
    public void UnverifiedR1ConnectFailsWithFeatureAndRevision()
    {
        var exception = Assert.Throws<SFSampFeatureUnavailableException>(
            () => SampObservationOffsets.Resolve(SFSampVersion.R1, SampObservationPoint.Connect));

        Assert.Equal(SFSampVersion.R1, exception.Version);
        Assert.Equal("Connect", exception.Feature);
        Assert.Contains("R1", exception.Message);
    }

    [Theory]
    [InlineData(SFSampVersion.Unknown)]
    [InlineData((SFSampVersion)999)]
    public void UnknownVersionsNeverBorrowAnotherRevision(SFSampVersion version)
    {
        foreach (SampObservationPoint point in Enum.GetValues<SampObservationPoint>())
            Assert.Throws<SFSampFeatureUnavailableException>(() => SampObservationOffsets.Resolve(version, point));
    }

    [Theory]
    [InlineData(SFSampVersion.R1, 0x139A0, 0xFDC0, 0x6CEA0)]
    [InlineData(SFSampVersion.R3_1, 0x16BB0, 0x12F20, 0x70D90)]
    [InlineData(SFSampVersion.R5_1, 0x17260, 0x13270, 0x71480)]
    public void RepresentativeBindingsMatchTheirNativeOwners(SFSampVersion version, int onFoot, int pickup, int selection)
    {
        Assert.Equal(onFoot, SampObservationOffsets.Resolve(version, SampObservationPoint.RemoteOnFoot));
        Assert.Equal(pickup, SampObservationOffsets.Resolve(version, SampObservationPoint.PickupCreate));
        Assert.Equal(selection, SampObservationOffsets.Resolve(version, SampObservationPoint.SelectionNotify));
    }

    [Fact]
    public void R5ActorAndLabelResetsKeepAuditedTargets()
    {
        Assert.Equal(0x9490, SampObservationOffsets.Resolve(SFSampVersion.R5_1, SampObservationPoint.ResetActors));
        Assert.Equal(0x93F0, SampObservationOffsets.Resolve(SFSampVersion.R5_1, SampObservationPoint.ResetLabels));
    }
}
