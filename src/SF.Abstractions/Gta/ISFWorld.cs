using System.Numerics;

namespace SFSharp.Abstractions.Gta;

/// <summary>Queries loaded GTA collision only. Game thread only; missing streamed collision is not a hit.</summary>
public interface ISFWorld
{
    bool TryRaycast(Vector3 origin, Vector3 target, SFRaycastOptions options, out SFRaycastHit hit);
    /// <summary>Searches building collision vertically below the supplied position, as CWorld::FindGroundZFor3DCoord does.</summary>
    bool TryFindGround(Vector3 position, out float height);
}

[Flags]
public enum SFRaycastOptions
{
    None = 0, Buildings = 1, Vehicles = 2, Peds = 4, Objects = 8, Dummies = 16,
    SeeThrough = 32, IgnoreCameraCheck = 64, ShootThrough = 128,
    AllEntities = Buildings | Vehicles | Peds | Objects | Dummies
}

/// <summary>Copied collision. Entity is null for buildings/dummies and other entities without a supported pool handle.</summary>
public readonly record struct SFRaycastHit(Vector3 Position, Vector3 Normal, byte Surface, SFEntityRef? Entity);
