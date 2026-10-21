using SFSharp.Abstractions;
using SFSharp.Abstractions.Gta;
using SFSharp.Abstractions.Samp;

namespace SF.Network.Contracts.Tests;

public sealed class EntityContractTests
{
    [Fact]
    public void RootFacadeExposesEntities()
    {
        Assert.Equal(typeof(ISFGta), typeof(ISF).GetProperty(nameof(ISF.Gta))?.PropertyType);
        Assert.Equal(typeof(ISFSamp), typeof(ISF).GetProperty(nameof(ISF.Samp))?.PropertyType);
        Assert.Equal(typeof(ISFEntities), typeof(ISFGta).GetProperty(nameof(ISFGta.Entities))?.PropertyType);
        Assert.Null(typeof(ISF).GetProperty("Entities"));
        Assert.Null(typeof(ISFEntities).GetMethod("TryGetPlayer"));
        Assert.NotNull(typeof(ISFSampEntities).GetMethod("TryGetPlayer"));
        Assert.Null(typeof(ISFCamera).GetProperty("AimDirection"));
        Assert.NotNull(typeof(ISFSampCamera).GetProperty("AimDirection"));
    }

    [Theory]
    [InlineData(SFEntityKind.Vehicle, 2)]
    [InlineData(SFEntityKind.Ped, 3)]
    [InlineData(SFEntityKind.Object, 4)]
    public void EntityKindMatchesEngineTypeStatus(SFEntityKind kind, byte engineType)
    {
        Assert.Equal(engineType, (byte)kind);
    }

    [Fact]
    public void EntityRefsCompareByKindAndHandle()
    {
        Assert.Equal(new SFEntityRef(SFEntityKind.Ped, 0x105), new SFEntityRef(SFEntityKind.Ped, 0x105));
        Assert.NotEqual(new SFEntityRef(SFEntityKind.Ped, 0x105), new SFEntityRef(SFEntityKind.Vehicle, 0x105));
    }
}
