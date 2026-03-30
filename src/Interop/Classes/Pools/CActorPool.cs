using SFSharp;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<CActorPool*, ActorInfo*, int>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<CActorPool*, ushort, int>;
using unsafe DoesExistDelegate = delegate* unmanaged[Thiscall]<CActorPool*, ushort, int>;
using unsafe FindDelegate = delegate* unmanaged[Thiscall]<CActorPool*, CPed*, ushort>;
using unsafe GetDelegate = delegate* unmanaged[Thiscall]<CActorPool*, ushort, nint>;

[StructLayout(LayoutKind.Explicit, Size = 20004, Pack = 1)]
public unsafe ref struct CActorPool
{
    private static CActorPool* CurrentInstance => CNetGame.Instance.GetActorPool();
    public static ref readonly CActorPool Instance => ref *RequireInstance();

    [FieldOffset(SampOffsets.CActorPool.LargestId)]
    private readonly int _largestId;

    public int LargestId => RequireInstance()->_largestId;

    private static readonly GetDelegate _get = (GetDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActorPool.Get);
    private static readonly DoesExistDelegate _doesExist = (DoesExistDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActorPool.DoesExist);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActorPool.Delete);
    private static readonly FindDelegate _find = (FindDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActorPool.Find);
    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActorPool.Create);

    public bool IsValidId(ushort actorId) => actorId < SampOffsets.CActorPool.MaxActors;

    public CActor* Get(ushort actorId)
    {
        return (CActor*)_get(RequireInstance(), actorId);
    }

    public bool DoesExist(ushort actorId)
    {
        return IsValidId(actorId) && _doesExist(RequireInstance(), actorId) != 0;
    }

    public bool TryGet(ushort actorId, out CActor* actor)
    {
        actor = null;
        if (!DoesExist(actorId))
        {
            return false;
        }

        actor = Get(actorId);
        return actor != null;
    }

    public bool Delete(ushort actorId)
    {
        return _delete(RequireInstance(), actorId) != 0;
    }

    public ushort Find(CPed* gamePed)
    {
        return _find(RequireInstance(), gamePed);
    }

    public bool Create(in ActorInfo info)
    {
        ActorInfo copy = info;
        return _create(RequireInstance(), &copy) != 0;
    }

    public ushort[] GetExistingIds()
    {
        List<ushort> ids = [];
        for (ushort actorId = 0; actorId < SampOffsets.CActorPool.MaxActors; actorId++)
        {
            if (DoesExist(actorId))
            {
                ids.Add(actorId);
            }
        }

        return [.. ids];
    }

    private static CActorPool* RequireInstance()
    {
        CActorPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CActorPool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorInfo
{
    public ushort Id;
    public int Model;
    public Vector3 Position;
    public float Rotation;
    public float Health;
    public byte Invulnerable;
}
