using System.Numerics;
using SFSharp.Runtime.Interop.Engine;

namespace SF.Network.Tests;

public sealed unsafe class GtaCameraTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReadsSelectedCameraAndNativeFieldTypes(byte index)
    {
        byte* memory = stackalloc byte[0xD78];
        new Span<byte>(memory, 0xD78).Clear();
        memory[0x59] = index;
        byte* cam = memory + 0x174 + index * 0x238;
        *(Vector3*)(cam + 0x19C) = new(1, 2, 3);
        *(Vector3*)(cam + 0x190) = Vector3.UnitY;
        *(Vector3*)(cam + 0x1B4) = Vector3.UnitZ;
        *(float*)(cam + 0xB4) = 70;
        *(ushort*)(cam + 0xC) = 300;
        *(float*)(memory + 0xBFC) = 127.5f;
        memory[0x3D] = 1;
        *(float*)(memory + 0x12C) = 0.75f;

        GtaCameraView view = new((nint)memory);

        Assert.True(view.HasActiveCamera);
        Assert.Equal(new Vector3(1, 2, 3), view.Position);
        Assert.Equal(Vector3.UnitY, view.Front);
        Assert.Equal(Vector3.UnitZ, view.Up);
        Assert.Equal(70, view.FieldOfView);
        Assert.Equal((ushort)300, view.Mode);
        Assert.Equal(127.5f, view.FadeAlpha);
        Assert.Equal((byte)1, view.FadeState);
        Assert.True(view.IsWideScreen);
        Assert.Equal(0.75f, view.ShakeForce);
    }

    [Fact]
    public void RejectsInvalidCameraIndex()
    {
        byte* memory = stackalloc byte[0xD78];
        memory[0x59] = 3;
        GtaCameraView view = new((nint)memory);

        Assert.False(view.HasActiveCamera);
        Assert.Throws<InvalidOperationException>(() => view.Position);
    }
}
