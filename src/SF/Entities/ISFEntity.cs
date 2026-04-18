namespace SFSharp.Runtime.Game.Entities;

public interface ISFEntity
{
    nint NativePointer { get; }
    nint GamePointer { get; }
    int Handle { get; }
    bool IsAvailable { get; }
}
