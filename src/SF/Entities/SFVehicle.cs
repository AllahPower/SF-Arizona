namespace SFSharp.Runtime.Game;

public sealed unsafe class SFVehicle : SFEntity
{
    private readonly ushort? _id;
    private readonly CVehicle* _native;

    internal SFVehicle(CVehicle* native)
        : this(null, native)
    {
    }

    internal SFVehicle(ushort? id, CVehicle* native)
    {
        _id = id;
        _native = native;
    }

    public ushort? Id => _id;
    public CVehicle* Native => _native;
    public nint GameVehiclePointer => _native is null ? 0 : _native->GameVehicle;
    public float Health => _native is null ? 0f : _native->GetHealth();
    public bool IsInvulnerable => _native is not null && _native->IsInvulnerable;
    public bool IsLightsOn => _native is not null && _native->IsLightsOn;
    public bool IsLocked => _native is not null && _native->IsLocked;
    public bool EngineState => _native is not null && _native->EngineState;
    public bool HasDriver => _native is not null && _native->HasDriver();
    public bool IsOccupied => _native is not null && _native->IsOccupied();
    public new bool Exists => _native is not null && _native->DoesExist();
    public bool SirenEnabled => _native is not null && _native->SirenEnabled();
    public byte PrimaryColor => _native is null ? byte.MaxValue : _native->PrimaryColor;
    public byte SecondaryColor => _native is null ? byte.MaxValue : _native->SecondaryColor;
    public bool HasTrailer => _native is not null && _native->Trailer is not null;
    public SFVehicle? Trailer => _native is null || _native->Trailer is null ? null : new SFVehicle(null, _native->Trailer);

    protected override CEntity* NativeEntity => (CEntity*)_native;

    public void SetHealth(float value)
    {
        if (_native is not null)
        {
            _native->SetHealth(value);
        }
    }

    public void SetInvulnerable(bool value)
    {
        if (_native is not null)
        {
            _native->SetInvulnerable(value);
        }
    }

    public void SetLocked(bool value)
    {
        if (_native is not null)
        {
            _native->SetLocked(value);
        }
    }

    public void SetRotation(float value)
    {
        if (_native is not null)
        {
            _native->SetRotation(value);
        }
    }

    public void EnableEngine(bool enable)
    {
        if (_native is not null)
        {
            _native->EnableEngine(enable);
        }
    }

    public void EnableSiren(bool enable)
    {
        if (_native is not null)
        {
            _native->EnableSiren(enable);
        }
    }

    public void SetColor(byte primaryColor, byte secondaryColor)
    {
        if (_native is not null)
        {
            _native->SetColor(primaryColor, secondaryColor);
            _native->UpdateColor();
        }
    }

    public void SetLicensePlateText(string text)
    {
        if (_native is not null)
        {
            _native->SetLicensePlateText(text);
        }
    }
}
