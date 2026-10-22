using System.Numerics;

namespace SFSharp.Abstractions.Samp.Events;

public enum SFSampSyncKind : byte { OnFoot, Vehicle, Passenger, Aim, Trailer }
public enum SFSampRemoteAction : byte { EnterVehicle, ExitVehicle, Kill }

/// <summary>Copied client bookkeeping, not a prediction of the rendered ped position.</summary>
public readonly record struct SFSampRemoteSnapshot(
    ushort PlayerId, ushort VehicleId, byte State, byte SeatId, uint LastUpdate, uint LastTimestamp,
    float Health, float Armour, Vector3 OnFootTarget, Vector3 VehicleTarget);

/// <summary>Native Update completed. Stored is present for onfoot/vehicle/passenger only.</summary>
/// <param name="Kind">Native sync routine kind.</param>
/// <param name="Before">Client bookkeeping before the native call.</param>
/// <param name="After">Client bookkeeping after the native call.</param>
/// <param name="NativeTimestamp">Native routine argument, not a guaranteed wire ID_TIMESTAMP. Absent for aim, trailer and unused passenger time.</param>
/// <param name="Input">Owned copy of decoded native input, or empty when the routine received a null input.</param>
/// <param name="Stored">Owned copy of available client cache data after processing.</param>
/// <param name="StoredDataChanged">Whether stored bytes changed, not whether the server accepted the action.</param>
public readonly record struct SFSampRemoteSyncEvent(
    SFSampSyncKind Kind, SFSampRemoteSnapshot Before, SFSampRemoteSnapshot After, uint? NativeTimestamp,
    SFSampSyncPayload Input, SFSampSyncPayload Stored, bool StoredDataChanged);

public readonly record struct SFSampRemoteStateEvent(ushort PlayerId, byte? Previous, byte Current);
public readonly record struct SFSampRemoteActionEvent(
    ushort PlayerId, SFSampRemoteAction Action, ushort VehicleId, bool IsPassenger, SFSampRemoteSnapshot After);
