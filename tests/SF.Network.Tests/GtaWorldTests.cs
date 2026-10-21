using System.Numerics;
using System.Runtime.InteropServices;
using SFSharp.Abstractions.Gta;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Gta;
using SFSharp.Runtime.Interop.Engine;
using SFSharp.Runtime.Interop.Gta;

namespace SF.Network.Tests;

public sealed class GtaWorldTests
{
    [Fact]
    public void CollisionLayoutMatchesNativeQueryOutput()
    {
        Assert.Equal(44, Marshal.SizeOf<GtaCollisionPoint>());
        Assert.Equal(0x10, Marshal.OffsetOf<GtaCollisionPoint>(nameof(GtaCollisionPoint.Normal)).ToInt32());
        Assert.Equal(0x23, Marshal.OffsetOf<GtaCollisionPoint>(nameof(GtaCollisionPoint.SurfaceB)).ToInt32());
    }

    [Fact]
    public void RejectsNonFiniteInputsAndDegenerateRaysBeforeNativeCall()
    {
        MainThreadDispatcher main = new(new SFSynchronizationContext(ex => throw ex));
        SFGtaWorld world = new(main);

        Assert.False(world.TryRaycast(Vector3.One, Vector3.One, SFRaycastOptions.AllEntities, out _));
        Assert.Throws<ArgumentException>(() => world.TryFindGround(new(float.NaN, 0, 0), out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => world.TryRaycast(Vector3.Zero, Vector3.One, (SFRaycastOptions)256, out _));
        Assert.Throws<ArgumentException>(() => new SFGtaCamera(main).TryWorldToScreen(new(0, float.PositiveInfinity, 0), out _, out _));
    }

    [Theory]
    [InlineData(0x105, 0x05, true)]
    [InlineData(0x105, 0x06, false)]
    [InlineData(0x105, 0x85, false)]
    public void StaleOrFreePoolSlotsDoNotResolve(int handle, byte map, bool valid)
    {
        Assert.Equal(valid, GtaPoolHandle.Matches(handle, map));
    }
}
