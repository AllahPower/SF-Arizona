using System.Numerics;

namespace SFSharp.Runtime.Gta;

public sealed class SFGtaWorld(ISFMainThread mainThread) : ISFWorld
{
    public bool TryRaycast(Vector3 origin, Vector3 target, SFRaycastOptions options, out SFRaycastHit hit)
    {
        RequireThread();
        Validate(origin);
        Validate(target);
        if (((int)options & ~255) != 0) throw new ArgumentOutOfRangeException(nameof(options));
        if (origin == target) { hit = default; return false; }
        if (!GtaWorldQueries.IsWorldReady) { hit = default; return false; }
        return GtaWorldQueries.Raycast(origin, target, options, out hit);
    }

    public bool TryFindGround(Vector3 position, out float height)
    {
        RequireThread();
        Validate(position);
        if (!GtaWorldQueries.IsWorldReady) { height = default; return false; }
        return GtaWorldQueries.Ground(position, out height);
    }

    private void RequireThread()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("GTA world queries require the game thread.");
    }

    internal static void Validate(Vector3 point)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || !float.IsFinite(point.Z))
            throw new ArgumentException("World coordinates must be finite.", nameof(point));
    }
}
