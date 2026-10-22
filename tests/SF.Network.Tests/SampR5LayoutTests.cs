using System.Runtime.InteropServices;
using SFSharp.Runtime.Interop.Classes.Players;
using SFSharp.Runtime.Interop.Classes.Pools;
using SFSharp.Runtime.Interop.Classes.Vehicles;

namespace SF.Network.Tests;

public sealed class SampR5LayoutTests
{
    [Theory]
    [InlineData(typeof(CVehicle), "_isLightsOn", 0x54)]
    [InlineData(typeof(CVehicle), "_engineState", 0x75)]
    [InlineData(typeof(CVehicle), "_primaryColor", 0x79)]
    [InlineData(typeof(CVehicle), "_secondaryColor", 0x7A)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.incarData), 0)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.onfootData), 0x94)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.currentVehicleId), 0xF8)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.head), 0x12B)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.classSelection), 0x2FA)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.spectating), 0x30E)]
    [InlineData(typeof(LocalPlayerR5), nameof(LocalPlayerR5.damage), 0x318)]
    [InlineData(typeof(RemotePlayerR5), nameof(RemotePlayerR5.id), 0x1E5)]
    [InlineData(typeof(RemotePlayerR5), nameof(RemotePlayerR5.vehicleId), 0x1E7)]
    [InlineData(typeof(RemotePlayerR5), nameof(RemotePlayerR5.animation), 0x1B4)]
    [InlineData(typeof(RemotePlayerR5), nameof(RemotePlayerR5.lastTimestamp), 0x1BD)]
    [InlineData(typeof(RemotePlayerR5), nameof(RemotePlayerR5.head), 0x1C9)]
    [InlineData(typeof(PlayerPoolR5), nameof(PlayerPoolR5.Ping), 0x22)]
    [InlineData(typeof(PlayerPoolR5), nameof(PlayerPoolR5.Player), 0x26)]
    [InlineData(typeof(PlayerInfoR5), nameof(PlayerInfoR5.Player), 0x10)]
    [InlineData(typeof(PlayerInfoR5), nameof(PlayerInfoR5.Name), 0x18)]
    [InlineData(typeof(MenuR5), nameof(MenuR5.Columns), 0x38C)]
    [InlineData(typeof(MenuR5), nameof(MenuR5.Interaction), 0x38D)]
    [InlineData(typeof(MenuR5), nameof(MenuR5.Panel), 0x3CF)]
    public void NativeFieldsMatchAuditedPackedOffsets(Type type, string field, int expectedOffset)
        => Assert.Equal(expectedOffset, Marshal.OffsetOf(type, field).ToInt32());
}
