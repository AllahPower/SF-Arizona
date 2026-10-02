using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

// gta_sa.exe 1.0 US damage entry points, verified in IDA (MD5 84a781f30fac7eba030af2497da86930):
// argument sizes match the callee cleanup (retn 0Ch / 18h / 14h). InflictDamage and ObjectDamage start
// with push/nop + jmp into an executable protection stub, which MinHook relocates into the trampoline.

internal readonly record struct PedDamageArgs(nint Calculator, nint Ped, nint Response, byte Speak);

/// <summary><c>CPedDamageResponseCalculator::ComputeDamageResponse(CPed*, CPedDamageResponse&amp;, bool)</c>.</summary>
internal sealed class PedDamageHook : NativeHook<PedDamageArgs, NoRetValue, PedDamageHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint calculator, nint ped, nint response, byte speak);

    private static PedDamageHook? _instance;

    public PedDamageHook()
    {
        _instance = this;
        InstallHook(DamageHookTargets.Resolve(GtaOffsets.DamageRva.ComputePedDamageResponse), new Native(HookProc));
    }

    private static void HookProc(nint calculator, nint ped, nint response, byte speak) => _instance!.Process(new(calculator, ped, response, speak));

    protected override NoRetValue InvokeOriginalFunction(PedDamageArgs args)
    {
        OriginalFunction(args.Calculator, args.Ped, args.Response, args.Speak);
        return default;
    }
}

internal readonly record struct VehicleDamageArgs(nint Vehicle, nint Source, int Weapon, float Amount, Vector3 Position);

/// <summary><c>CVehicle::InflictDamage(CEntity*, eWeaponType, float, CVector)</c>.</summary>
internal sealed class VehicleDamageHook : NativeHook<VehicleDamageArgs, NoRetValue, VehicleDamageHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint vehicle, nint source, int weapon, float amount, Vector3 position);

    private static VehicleDamageHook? _instance;

    public VehicleDamageHook()
    {
        _instance = this;
        InstallHook(DamageHookTargets.Resolve(GtaOffsets.DamageRva.InflictVehicleDamage), new Native(HookProc));
    }

    private static void HookProc(nint vehicle, nint source, int weapon, float amount, Vector3 position)
    {
        _instance!.Process(new(vehicle, source, weapon, amount, position));
    }

    protected override NoRetValue InvokeOriginalFunction(VehicleDamageArgs args)
    {
        OriginalFunction(args.Vehicle, args.Source, args.Weapon, args.Amount, args.Position);
        return default;
    }
}

internal readonly record struct ObjectDamageArgs(nint Object, float Amount, nint CollisionPosition, nint CollisionDirection, nint Source, int Weapon);

/// <summary><c>CObject::ObjectDamage(float, CVector*, CVector*, CEntity*, eWeaponType)</c>.</summary>
internal sealed class ObjectDamageHook : NativeHook<ObjectDamageArgs, NoRetValue, ObjectDamageHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void Native(nint obj, float amount, nint collisionPosition, nint collisionDirection, nint source, int weapon);

    private static ObjectDamageHook? _instance;

    public ObjectDamageHook()
    {
        _instance = this;
        InstallHook(DamageHookTargets.Resolve(GtaOffsets.DamageRva.ObjectDamage), new Native(HookProc));
    }

    private static void HookProc(nint obj, float amount, nint collisionPosition, nint collisionDirection, nint source, int weapon)
    {
        _instance!.Process(new(obj, amount, collisionPosition, collisionDirection, source, weapon));
    }

    protected override NoRetValue InvokeOriginalFunction(ObjectDamageArgs args)
    {
        OriginalFunction(args.Object, args.Amount, args.CollisionPosition, args.CollisionDirection, args.Source, args.Weapon);
        return default;
    }
}

internal static unsafe class DamageHookTargets
{
    /// <summary>Resolves a game RVA and logs its first bytes, so a detour already placed by another mod shows up in the log.</summary>
    public static nint Resolve(int rva)
    {
        nint address = ModuleResolver.GetGameAddress(rva);
        ReadOnlySpan<byte> prologue = new((void*)address, 8);
        SFLog.Debug($"Damage hook target 0x{address:X8} prologue={Convert.ToHexString(prologue)}");
        return address;
    }
}
