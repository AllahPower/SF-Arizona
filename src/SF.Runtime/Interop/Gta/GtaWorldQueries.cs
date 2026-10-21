using System.Numerics;

namespace SFSharp.Runtime.Interop.Gta;

internal static unsafe class GtaWorldQueries
{
    internal static bool IsWorldReady => GameLoadHooks.GameState == 9;
    internal static bool Raycast(Vector3 origin, Vector3 target, SFRaycastOptions flags, out SFRaycastHit hit)
    {
        GtaCollisionPoint point = default;
        GtaEntity* entity = null;
        var call = (delegate* unmanaged[Cdecl]<Vector3*, Vector3*, GtaCollisionPoint*, GtaEntity**, byte, byte, byte, byte, byte, byte, byte, byte, byte>)
            ModuleResolver.GetGameAddress(GtaOffsets.WorldRva.ProcessLineOfSight);
        bool found = call(&origin, &target, &point, &entity,
            Bit(flags, SFRaycastOptions.Buildings), Bit(flags, SFRaycastOptions.Vehicles), Bit(flags, SFRaycastOptions.Peds),
            Bit(flags, SFRaycastOptions.Objects), Bit(flags, SFRaycastOptions.Dummies), Bit(flags, SFRaycastOptions.SeeThrough),
            Bit(flags, SFRaycastOptions.IgnoreCameraCheck), Bit(flags, SFRaycastOptions.ShootThrough)) != 0;
        hit = found ? new(point.Position, point.Normal, point.SurfaceB, ToReference(entity)) : default;
        return found;
    }

    internal static bool Ground(Vector3 position, out float height)
    {
        byte found = 0;
        GtaEntity* entity = null;
        var call = (delegate* unmanaged[Cdecl]<float, float, float, byte*, GtaEntity**, float>)
            ModuleResolver.GetGameAddress(GtaOffsets.WorldRva.FindGroundZFor3DCoord);
        float result = call(position.X, position.Y, position.Z, &found, &entity);
        height = found != 0 ? result : default;
        return found != 0;
    }

    internal static bool Project(Vector3 position, out Vector2 screen, out float depth)
    {
        Vector3 projected = default;
        float scaleX, scaleY;
        var call = (delegate* unmanaged[Cdecl]<Vector3*, Vector3*, float*, float*, byte, byte, byte>)
            ModuleResolver.GetGameAddress(GtaOffsets.WorldRva.CalcScreenCoors);
        bool found = call(&position, &projected, &scaleX, &scaleY, 1, 1) != 0;
        screen = found ? new(projected.X, projected.Y) : default;
        depth = found ? projected.Z : default;
        return found;
    }

    private static byte Bit(SFRaycastOptions value, SFRaycastOptions flag) => (byte)((value & flag) != 0 ? 1 : 0);

    private static SFEntityRef? ToReference(GtaEntity* entity)
    {
        if (entity is null) return null;
        SFEntityKind kind = (SFEntityKind)(byte)entity->Type;
        return kind is SFEntityKind.Ped or SFEntityKind.Vehicle or SFEntityKind.Object
            && SFEntities.TryCreateRef(kind, (nint)entity, out SFEntityRef reference) ? reference : null;
    }
}
