using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SFSharp.Runtime.Interop.Engine;

namespace SF.Network.Tests;

// Expected values are copied from the gta_sa.exe 1.0 US IDA database (MD5 84a781f30fac7eba030af2497da86930),
// not from GtaOffsets, so an accidental constant edit fails here.
public sealed unsafe class GtaEngineLayoutTests
{
    [Theory]
    [InlineData(typeof(GtaPlaceable), 0x18)]
    [InlineData(typeof(GtaMatrix), 0x48)]
    [InlineData(typeof(GtaEntity), 0x38)]
    [InlineData(typeof(GtaPhysical), 0x138)]
    [InlineData(typeof(GtaPed), 0x79C)]
    [InlineData(typeof(GtaVehicle), 0x5A0)]
    [InlineData(typeof(GtaObject), 0x17C)]
    [InlineData(typeof(GtaPool), 0x14)]
    public void StructSizesMatchGame(Type type, int expectedSize)
    {
        Assert.Equal(expectedSize, Marshal.SizeOf(type));
    }

    [Theory]
    [InlineData(typeof(GtaPlaceable), nameof(GtaPlaceable.SimplePosition), 0x04)]
    [InlineData(typeof(GtaPlaceable), nameof(GtaPlaceable.SimpleAngle), 0x10)]
    [InlineData(typeof(GtaPlaceable), nameof(GtaPlaceable.Matrix), 0x14)]
    [InlineData(typeof(GtaMatrix), nameof(GtaMatrix.Up), 0x10)]
    [InlineData(typeof(GtaMatrix), nameof(GtaMatrix.At), 0x20)]
    [InlineData(typeof(GtaMatrix), nameof(GtaMatrix.Position), 0x30)]
    [InlineData(typeof(GtaEntity), nameof(GtaEntity.RwObject), 0x18)]
    [InlineData(typeof(GtaEntity), nameof(GtaEntity.Flags), 0x1C)]
    [InlineData(typeof(GtaEntity), nameof(GtaEntity.ModelIndex), 0x22)]
    [InlineData(typeof(GtaEntity), nameof(GtaEntity.Interior), 0x2F)]
    [InlineData(typeof(GtaEntity), nameof(GtaEntity.TypeStatus), 0x36)]
    [InlineData(typeof(GtaPhysical), nameof(GtaPhysical.MoveSpeed), 0x44)]
    [InlineData(typeof(GtaPhysical), nameof(GtaPhysical.TurnSpeed), 0x50)]
    [InlineData(typeof(GtaPhysical), nameof(GtaPhysical.Mass), 0x8C)]
    [InlineData(typeof(GtaPhysical), nameof(GtaPhysical.AttachedTo), 0xFC)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.Flags), 0x46C)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.PedState), 0x530)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.Health), 0x540)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.MaxHealth), 0x544)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.Armour), 0x548)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.CurrentRotation), 0x558)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.AimingRotation), 0x55C)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.Vehicle), 0x58C)]
    [InlineData(typeof(GtaPed), nameof(GtaPed.ActiveWeaponSlot), 0x718)]
    [InlineData(typeof(GtaVehicle), nameof(GtaVehicle.PrimaryColor), 0x434)]
    [InlineData(typeof(GtaVehicle), nameof(GtaVehicle.Driver), 0x460)]
    [InlineData(typeof(GtaVehicle), nameof(GtaVehicle.Passengers), 0x464)]
    [InlineData(typeof(GtaVehicle), nameof(GtaVehicle.MaxPassengers), 0x488)]
    [InlineData(typeof(GtaVehicle), nameof(GtaVehicle.Health), 0x4C0)]
    [InlineData(typeof(GtaVehicle), nameof(GtaVehicle.DoorLock), 0x4F8)]
    [InlineData(typeof(GtaObject), nameof(GtaObject.ObjectType), 0x13C)]
    [InlineData(typeof(GtaObject), nameof(GtaObject.Health), 0x154)]
    [InlineData(typeof(GtaPool), nameof(GtaPool.ByteMap), 0x04)]
    [InlineData(typeof(GtaPool), nameof(GtaPool.Capacity), 0x08)]
    public void FieldOffsetsMatchGame(Type type, string field, int expectedOffset)
    {
        Assert.Equal(expectedOffset, (int)Marshal.OffsetOf(type, field));
    }

    [Fact]
    public void PassengerArrayHoldsEightPointers()
    {
        Assert.Equal(8 * sizeof(nint), Unsafe.SizeOf<GtaPedPointerArray8>());
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(90f)]
    [InlineData(-135f)]
    [InlineData(179f)]
    public void HeadingInvertsSetRotateZOnly(float degrees)
    {
        float radians = degrees * MathF.PI / 180f;
        // CMatrix::SetRotateZOnly (0x59B020): right = (cos, sin, 0), up = (-sin, cos, 0).
        GtaMatrix matrix = new()
        {
            Right = new Vector3(MathF.Cos(radians), MathF.Sin(radians), 0f),
            Up = new Vector3(-MathF.Sin(radians), MathF.Cos(radians), 0f),
            At = Vector3.UnitZ,
            Position = new Vector3(1f, 2f, 3f),
        };
        GtaPlaceable placeable = new() { Matrix = &matrix };

        Assert.Equal(degrees, placeable.HeadingDegrees, 3);
        Assert.Equal(new Vector3(1f, 2f, 3f), placeable.Position);
    }

    [Fact]
    public void SimpleTransformIsUsedWithoutMatrix()
    {
        GtaPlaceable placeable = new() { SimplePosition = new Vector3(4f, 5f, 6f), SimpleAngle = MathF.PI / 2f };

        Assert.Equal(new Vector3(4f, 5f, 6f), placeable.Position);
        Assert.Equal(90f, placeable.HeadingDegrees, 3);
        Matrix4x4 matrix = placeable.GetMatrix();
        Assert.Equal(new Vector3(4f, 5f, 6f), matrix.Translation);
        Assert.Equal(-1f, matrix.M21, 5);
    }

    [Fact]
    public void EntityTypeComesFromLowBitsOfTypeStatus()
    {
        GtaEntity entity = new() { TypeStatus = (1 << 3) | 3 };

        Assert.Equal(GtaEntityType.Ped, entity.Type);
    }

    [Fact]
    public void HandleEncodesSlotAndGenerationLikeGetPedRef()
    {
        int handle = GtaPoolHandle.Compose(37, 0x12);

        Assert.Equal((37 << 8) + 0x12, handle);
        Assert.Equal(37, GtaPoolHandle.GetSlot(handle));
        Assert.Equal(0x12, GtaPoolHandle.GetGeneration(handle));
        Assert.True(GtaPoolHandle.IsInRange(handle, 38));
        Assert.False(GtaPoolHandle.IsInRange(handle, 37));
        Assert.False(GtaPoolHandle.IsInRange(-1, 38));
    }

    [Fact]
    public void OccupiedHandlesSkipFreeSlots()
    {
        byte[] byteMap = [0x05, 0x80 | 0x06, 0x00, 0x81, 0x7F];
        fixed (byte* map = byteMap)
        {
            GtaPool pool = new() { ByteMap = map, Capacity = byteMap.Length };

            List<int> handles = GtaPools.GetOccupiedHandles(&pool);

            Assert.Equal([0x005, 0x200, 0x47F], handles);
        }
    }
}
