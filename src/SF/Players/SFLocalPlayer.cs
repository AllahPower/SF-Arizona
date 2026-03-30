namespace SFSharp;

public sealed unsafe class SFLocalPlayer : SFPlayer
{
    internal SFLocalPlayer()
    {
    }

    public override ushort Id => CPlayerPool.Instance.LocalPlayerId;
    public override bool IsLocal => true;
    public override bool IsConnected => CPlayerPool.Instance.TryGetLocalPlayer(out _);
    public override bool IsNpc => false;

    public override SFPed? Ped
    {
        get
        {
            if (!CPlayerPool.Instance.TryGetLocalPlayer(out CLocalPlayer* localPlayer))
                return null;

            CPed* ped = localPlayer->GetPed();
            return ped is not null && SFPed.TryCreate(ped, this, out SFPed wrappedPed) ? wrappedPed : null;
        }
    }

    public byte CurrentWeapon => CLocalPlayer.Instance.CurrentWeapon;
    public byte SpecialAction => CLocalPlayer.Instance.GetSpecialAction();
    public ushort? AimedPlayerId => SF.Players.GetAimedPlayerId();
    public ushort AimedActorId => CLocalPlayer.Instance.AimedActorId;
    public SFVehicle? CurrentVehicle => Ped?.Vehicle;
    public ushort CurrentVehicleId => CLocalPlayer.Instance.CurrentVehicleId;
    public ushort LastVehicleId => CLocalPlayer.Instance.LastVehicleId;
    public byte Team => CLocalPlayer.Instance.Team;
    public bool HasSpawnInfo => CLocalPlayer.Instance.HasSpawnInfo;
    public bool IsPassengerDriveBy => CLocalPlayer.Instance.IsPassengerDriveBy;
    public byte CurrentInterior => CLocalPlayer.Instance.CurrentInterior;
    public bool IsInRcMode => CLocalPlayer.Instance.IsInRcMode;
    public SFLocalPlayerCameraTarget CameraTarget => new(
        CLocalPlayer.Instance.CameraTarget.ObjectId,
        CLocalPlayer.Instance.CameraTarget.VehicleId,
        CLocalPlayer.Instance.CameraTarget.PlayerId,
        CLocalPlayer.Instance.CameraTarget.ActorId);
    public SFPlayerHeadState Head => new(
        CLocalPlayer.Instance.Head.Direction,
        CLocalPlayer.Instance.Head.LastUpdate,
        CLocalPlayer.Instance.Head.LastLook);
    public SFLocalPlayerSurfingState Surfing => new(
        CLocalPlayer.Instance.Surfing.EntityId,
        CLocalPlayer.Instance.Surfing.LastUpdate,
        CLocalPlayer.Instance.Surfing.EntityPointer,
        CLocalPlayer.Instance.Surfing.IsStuck,
        CLocalPlayer.Instance.Surfing.IsActive,
        CLocalPlayer.Instance.Surfing.Position,
        CLocalPlayer.Instance.Surfing.Unknown0,
        CLocalPlayer.Instance.Surfing.Mode);
    public void RequestSpawn() => CLocalPlayer.Instance.RequestSpawn();
    public void SendChat(string text) => CLocalPlayer.Instance.Chat(text);
    public void SetColor(uint colorArgb) => CLocalPlayer.Instance.SetColor(colorArgb);
    public uint GetColorAsArgb() => CLocalPlayer.Instance.GetColorAsArgb();
    public uint GetColorAsRgba() => CLocalPlayer.Instance.GetColorAsRgba();

    public int GetWeaponAmmo(int slot) => CLocalPlayer.Instance.GetWeaponAmmo(slot);
    public byte GetWeaponId(int slot) => CLocalPlayer.Instance.GetWeaponId(slot);
    public string? GetNativeName() => CLocalPlayer.Instance.GetName();

    public SFVehicle? GetSurfingVehicle()
    {
        return CLocalPlayer.Instance.TryGetSurfingVehicle(out CVehicle* vehicle) ? new SFVehicle(null, vehicle) : null;
    }
}
