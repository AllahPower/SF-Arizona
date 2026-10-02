using SFSharp.Abstractions;
using SFSharp.Abstractions.Game;

namespace SF.Network.Contracts.Tests;

public sealed class EntityContractTests
{
    [Fact]
    public void RootFacadeExposesEntities()
    {
        Assert.Equal(typeof(ISFEntities), typeof(ISF).GetProperty(nameof(ISF.Entities))?.PropertyType);
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
