using System.Numerics;

namespace SFSharp.Runtime.Game.Players;

public sealed unsafe class SFRemotePlayer : SFPlayer
{
    private readonly ushort _id;

    internal SFRemotePlayer(ushort id)
    {
        _id = id;
    }

    public override ushort Id => _id;
    public override bool IsLocal => false;
    public override bool IsConnected => CPlayerPool.Instance.IsConnected(_id);

    public CRemotePlayer* Native => CPlayerPool.Instance.TryGetConnectedPlayer(_id, out CRemotePlayer* player) ? player : null;
    public bool Exists => Native is not null && Native->DoesExist();
    public float DistanceToLocalPlayer => Native is null ? 0f : Native->GetDistanceToLocalPlayer();
    public uint ColorRgba => Native is null ? 0u : Native->GetColorAsRgba();
    public uint ColorArgb => Native is null ? 0u : Native->GetColorAsArgb();
    public int Status => Native is null ? 0 : Native->GetStatus();
    public byte State => Native is null ? byte.MaxValue : Native->State;
    public byte SeatId => Native is null ? byte.MaxValue : Native->SeatId;
    public bool PassengerDriveBy => Native is not null && Native->PassengerDriveBy;
    public float ReportedHealth => Native is null ? 0f : Native->ReportedHealth;
    public float ReportedArmour => Native is null ? 0f : Native->ReportedArmour;
    public byte UpdateType => Native is null ? byte.MaxValue : Native->UpdateType;
    public uint LastUpdate => Native is null ? 0u : Native->LastUpdate;
    public uint LastTimestamp => Native is null ? 0u : Native->LastTimestamp;
    public bool PerformingCustomAnimation => Native is not null && Native->PerformingCustomAnimation;
    public SFPlayerHeadState Head => Native is null
        ? default
        : new SFPlayerHeadState(Native->Head.Direction, Native->Head.LastUpdate, Native->Head.LastLook);
    public SFRemotePlayerMarkerState Marker => Native is null
        ? default
        : new SFRemotePlayerMarkerState(Native->MarkerState, Native->MarkerPosition.ToVector3(), Native->MarkerHandle);
    public SFRemotePlayerTargetState TargetState => Native is null
        ? default
        : new SFRemotePlayerTargetState(
            Native->OnfootTargetPosition,
            Native->OnfootTargetSpeed,
            Native->IncarTargetPosition,
            Native->IncarTargetSpeed,
            Native->PositionDifference,
            Native->IncarTargetRotation.Real,
            Native->IncarTargetRotation.Imaginary);

    public override SFPed? Ped
    {
        get
        {
            CRemotePlayer* native = Native;
            if (native is null || !native->DoesExist() || native->Ped is null || !SFPed.TryCreate(native->Ped, this, out SFPed ped))
            {
                return null;
            }

            return ped;
        }
    }

    public override SFVehicle? Vehicle
    {
        get
        {
            CRemotePlayer* native = Native;
            if (native is null || native->Vehicle is null || !native->Vehicle->DoesExist())
            {
                return null;
            }

            ushort vehicleId = native->VehicleId;
            return new SFVehicle(vehicleId == ushort.MaxValue ? null : vehicleId, native->Vehicle);
        }
    }

    public ushort VehicleId => Native is null ? ushort.MaxValue : Native->VehicleId;
    public bool HasVehicle => VehicleId != ushort.MaxValue;
    public bool DrawLabels => Native is not null && Native->DrawLabels;
    public bool HasJetpack => Native is not null && Native->HasJetpack;
    public byte SpecialAction => Native is null ? byte.MaxValue : Native->SpecialAction;

    public void SetColor(uint colorRgba)
    {
        if (Native is not null)
        {
            Native->SetColor(colorRgba);
        }
    }
}
