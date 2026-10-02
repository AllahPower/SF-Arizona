using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

// SA-MP 0.3.7-R3-1 pool lifecycle entry points. Signatures follow SAMP-API src/0.3.7-R3-1; the callee
// stack cleanup (retn N) of every target was checked against the Arizona launcher samp.dll
// (MD5 e672e4723da63c93e984912f1193a798) on 2026-10-02.

internal interface ISampEntityLifecycleArgs
{
    SFSampEntityType Type { get; }
    ushort SampId { get; }
    SFSampEntityChange Change { get; }
}

/// <summary>
/// Raises <see cref="SFSampEntityEvent"/> around a lifecycle hook: removals before the original runs so
/// the entity can still be resolved, additions after it and only when it succeeded.
/// </summary>
internal sealed class SampEntityLifecycleSubHook<TArgs>(Action<TArgs, SFSampEntityChange> raise) : ISubHook<TArgs, bool>
    where TArgs : ISampEntityLifecycleArgs
{
    public bool Process(TArgs args, Func<TArgs, bool> next)
    {
        if (args.Change is SFSampEntityChange.Deleted or SFSampEntityChange.StreamedOut)
        {
            raise(args, args.Change);
            return next(args);
        }

        bool succeeded = next(args);
        if (succeeded)
        {
            // HookBase re-runs the original when a sub-hook throws, which would create the entity twice.
            try
            {
                raise(args, args.Change);
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"{typeof(TArgs).Name} {args.Change} event");
            }
        }

        return succeeded;
    }
}

internal readonly unsafe record struct VehiclePoolCreateArgs(nint Pool, nint Info) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Vehicle;
    public ushort SampId => *(ushort*)Info;
    public SFSampEntityChange Change => SFSampEntityChange.Created;
}

internal sealed class VehiclePoolCreateHook : NativeHook<VehiclePoolCreateArgs, bool, VehiclePoolCreateHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint pool, nint info);

    private static VehiclePoolCreateHook? _instance;

    public VehiclePoolCreateHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.Create), new Native(HookProc));
    }

    private static int HookProc(nint pool, nint info) => _instance!.Process(new(pool, info)) ? 1 : 0;

    protected override bool InvokeOriginalFunction(VehiclePoolCreateArgs args) => OriginalFunction(args.Pool, args.Info) != 0;
}

internal readonly record struct PoolDeleteArgs(nint Pool, ushort SampId, SFSampEntityType Type) : ISampEntityLifecycleArgs
{
    public SFSampEntityChange Change => SFSampEntityChange.Deleted;
}

/// <summary><c>CVehiclePool::Delete</c>, <c>CObjectPool::Delete</c> and <c>CActorPool::Delete</c> share <c>BOOL (ID)</c>.</summary>
internal sealed class PoolDeleteHook : NativeHook<PoolDeleteArgs, bool, PoolDeleteHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint pool, int id);

    private readonly SFSampEntityType _type;
    private readonly Native _detour;

    public PoolDeleteHook(SFSampEntityType type, int rva)
    {
        _type = type;
        _detour = HookProc;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", (uint)rva), _detour);
    }

    private int HookProc(nint pool, int id) => Process(new(pool, (ushort)id, _type)) ? 1 : 0;

    protected override bool InvokeOriginalFunction(PoolDeleteArgs args) => OriginalFunction(args.Pool, args.SampId) != 0;
}

internal readonly record struct ObjectPoolCreateArgs(nint Pool, ushort SampId, int Model, Vector3 Position, Vector3 Rotation, float DrawDistance) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Object;
    public SFSampEntityChange Change => SFSampEntityChange.Created;
}

internal sealed class ObjectPoolCreateHook : NativeHook<ObjectPoolCreateArgs, bool, ObjectPoolCreateHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint pool, int id, int model, Vector3 position, Vector3 rotation, float drawDistance);

    private static ObjectPoolCreateHook? _instance;

    public ObjectPoolCreateHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.Create), new Native(HookProc));
    }

    private static int HookProc(nint pool, int id, int model, Vector3 position, Vector3 rotation, float drawDistance)
    {
        return _instance!.Process(new(pool, (ushort)id, model, position, rotation, drawDistance)) ? 1 : 0;
    }

    protected override bool InvokeOriginalFunction(ObjectPoolCreateArgs args)
    {
        return OriginalFunction(args.Pool, args.SampId, args.Model, args.Position, args.Rotation, args.DrawDistance) != 0;
    }
}

internal readonly unsafe record struct ActorPoolCreateArgs(nint Pool, nint Info) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Actor;
    public ushort SampId => ((ActorInfo*)Info)->Id;
    public SFSampEntityChange Change => SFSampEntityChange.Created;
}

internal sealed class ActorPoolCreateHook : NativeHook<ActorPoolCreateArgs, bool, ActorPoolCreateHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint pool, nint info);

    private static ActorPoolCreateHook? _instance;

    public ActorPoolCreateHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CActorPool.Create), new Native(HookProc));
    }

    private static int HookProc(nint pool, nint info) => _instance!.Process(new(pool, info)) ? 1 : 0;

    protected override bool InvokeOriginalFunction(ActorPoolCreateArgs args) => OriginalFunction(args.Pool, args.Info) != 0;
}

internal readonly record struct PlayerPoolCreateArgs(nint Pool, ushort SampId, nint Name, int IsNpc) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Player;
    public SFSampEntityChange Change => SFSampEntityChange.Created;
}

internal sealed class PlayerPoolCreateHook : NativeHook<PlayerPoolCreateArgs, bool, PlayerPoolCreateHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint pool, int id, nint name, int isNpc);

    private static PlayerPoolCreateHook? _instance;

    public PlayerPoolCreateHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.Create), new Native(HookProc));
    }

    private static int HookProc(nint pool, int id, nint name, int isNpc) => _instance!.Process(new(pool, (ushort)id, name, isNpc)) ? 1 : 0;

    protected override bool InvokeOriginalFunction(PlayerPoolCreateArgs args) => OriginalFunction(args.Pool, args.SampId, args.Name, args.IsNpc) != 0;
}

internal readonly record struct PlayerPoolDeleteArgs(nint Pool, ushort SampId, int Reason) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Player;
    public SFSampEntityChange Change => SFSampEntityChange.Deleted;
}

internal sealed class PlayerPoolDeleteHook : NativeHook<PlayerPoolDeleteArgs, bool, PlayerPoolDeleteHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint pool, int id, int reason);

    private static PlayerPoolDeleteHook? _instance;

    public PlayerPoolDeleteHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPlayerPool.Delete), new Native(HookProc));
    }

    private static int HookProc(nint pool, int id, int reason) => _instance!.Process(new(pool, (ushort)id, reason)) ? 1 : 0;

    protected override bool InvokeOriginalFunction(PlayerPoolDeleteArgs args) => OriginalFunction(args.Pool, args.SampId, args.Reason) != 0;
}

internal readonly unsafe record struct RemotePlayerSpawnArgs(nint Player, int Team, int Model, int Unknown, nint Position, float Rotation, uint Color, int FightingStyle) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Player;
    public ushort SampId => ((CRemotePlayer*)Player)->Id;
    public SFSampEntityChange Change => SFSampEntityChange.StreamedIn;
}

internal sealed class RemotePlayerSpawnHook : NativeHook<RemotePlayerSpawnArgs, bool, RemotePlayerSpawnHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int Native(nint player, int team, int model, int unknown, nint position, float rotation, uint color, int fightingStyle);

    private static RemotePlayerSpawnHook? _instance;

    public RemotePlayerSpawnHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.Spawn), new Native(HookProc));
    }

    private static int HookProc(nint player, int team, int model, int unknown, nint position, float rotation, uint color, int fightingStyle)
    {
        return _instance!.Process(new(player, team, model, unknown, position, rotation, color, fightingStyle)) ? 1 : 0;
    }

    protected override bool InvokeOriginalFunction(RemotePlayerSpawnArgs args)
    {
        return OriginalFunction(args.Player, args.Team, args.Model, args.Unknown, args.Position, args.Rotation, args.Color, args.FightingStyle) != 0;
    }
}

internal readonly unsafe record struct RemotePlayerRemoveArgs(nint Player) : ISampEntityLifecycleArgs
{
    public SFSampEntityType Type => SFSampEntityType.Player;
    public ushort SampId => ((CRemotePlayer*)Player)->Id;
    public SFSampEntityChange Change => SFSampEntityChange.StreamedOut;
}

internal sealed class RemotePlayerRemoveHook : NativeHook<RemotePlayerRemoveArgs, bool, RemotePlayerRemoveHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint player);

    private static RemotePlayerRemoveHook? _instance;

    public RemotePlayerRemoveHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.Remove), new Native(HookProc));
    }

    private static void HookProc(nint player) => _instance!.Process(new(player));

    protected override bool InvokeOriginalFunction(RemotePlayerRemoveArgs args)
    {
        OriginalFunction(args.Player);
        return true;
    }
}
