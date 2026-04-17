using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe DoesExistDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CRemotePlayer*, int>;
using unsafe GetColorAsArgbDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CRemotePlayer*, uint>;
using unsafe GetColorAsRgbaDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CRemotePlayer*, uint>;
using unsafe GetDistanceToLocalPlayerDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CRemotePlayer*, float>;
using unsafe GetStatusDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CRemotePlayer*, int>;
using unsafe SetColorDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CRemotePlayer*, uint, void>;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Explicit, Size = 509, Pack = 1)]
public unsafe struct CRemotePlayer
{
    [FieldOffset(0x00)]
    private readonly CPed* _ped;

    [FieldOffset(0x04)]
    private readonly CVehicle* _vehicle;

    [FieldOffset(0x08)]
    private readonly ushort _id;

    [FieldOffset(0x0A)]
    private readonly ushort _vehicleId;

    [FieldOffset(0x10)]
    private readonly int _drawLabels;

    [FieldOffset(0x14)]
    private readonly int _hasJetpack;

    [FieldOffset(0x18)]
    private readonly byte _specialAction;

    [FieldOffset(0x19)]
    private readonly SampIncarData _incarData;

    [FieldOffset(0x58)]
    private readonly SampTrailerData _trailerData;

    [FieldOffset(0x8E)]
    private readonly SampAimData _aimData;

    [FieldOffset(0xAD)]
    private readonly SampPassengerData _passengerData;

    [FieldOffset(0xC5)]
    private readonly SampOnfootData _onfootData;

    [FieldOffset(0x109)]
    private readonly byte _team;

    [FieldOffset(0x10A)]
    private readonly byte _state;

    [FieldOffset(0x10B)]
    private readonly byte _seatId;

    [FieldOffset(0x110)]
    private readonly int _passengerDriveByRaw;

    [FieldOffset(0x114)]
    private readonly Vector3 _onfootTargetPosition;

    [FieldOffset(0x120)]
    private readonly Vector3 _onfootTargetSpeed;

    [FieldOffset(0x12C)]
    private readonly Vector3 _incarTargetPosition;

    [FieldOffset(0x138)]
    private readonly Vector3 _incarTargetSpeed;

    [FieldOffset(0x190)]
    private readonly Vector3 _positionDifference;

    [FieldOffset(0x19C)]
    private readonly CRemotePlayerIncarTargetRotation _incarTargetRotation;

    [FieldOffset(0x1AC)]
    private readonly float _reportedArmour;

    [FieldOffset(0x1B0)]
    private readonly float _reportedHealth;

    [FieldOffset(0x1C0)]
    private readonly SampAnimation _animation;

    [FieldOffset(0x1C4)]
    private readonly byte _updateType;

    [FieldOffset(0x1C5)]
    private readonly uint _lastUpdate;

    [FieldOffset(0x1C9)]
    private readonly uint _lastTimestamp;

    [FieldOffset(0x1CD)]
    private readonly int _performingCustomAnimationRaw;

    [FieldOffset(0x1D5)]
    private readonly CPlayerHeadState _head;

    [FieldOffset(0x1E9)]
    private readonly int _markerStateRaw;

    [FieldOffset(0x1ED)]
    private readonly CRemotePlayerMarkerPosition _markerPosition;

    [FieldOffset(0x1F9)]
    private readonly int _markerHandle;

    private static readonly GetDistanceToLocalPlayerDelegate _getDistanceToLocalPlayer = (GetDistanceToLocalPlayerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.GetDistanceToLocalPlayer);
    private static readonly SetColorDelegate _setColor = (SetColorDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.SetColor);
    private static readonly GetColorAsRgbaDelegate _getColorAsRgba = (GetColorAsRgbaDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.GetColorAsRgba);
    private static readonly GetColorAsArgbDelegate _getColorAsArgb = (GetColorAsArgbDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.GetColorAsArgb);
    private static readonly GetStatusDelegate _getStatus = (GetStatusDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.GetStatus);
    private static readonly DoesExistDelegate _doesExist = (DoesExistDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CRemotePlayer.DoesExist);

    public CPed* Ped => _ped;
    public CVehicle* Vehicle => _vehicle;
    public ushort Id => _id;
    public ushort VehicleId => _vehicleId;
    public bool DrawLabels => _drawLabels != 0;
    public bool HasJetpack => _hasJetpack != 0;
    public byte SpecialAction => _specialAction;
    public bool IsAvailable => _ped != null || _vehicle != null;
    public SampIncarData IncarData => _incarData;
    public SampTrailerData TrailerData => _trailerData;
    public SampAimData AimData => _aimData;
    public SampPassengerData PassengerData => _passengerData;
    public SampOnfootData OnfootData => _onfootData;
    public byte Team => _team;
    public byte State => _state;
    public byte SeatId => _seatId;
    public bool PassengerDriveBy => _passengerDriveByRaw != 0;
    public Vector3 OnfootTargetPosition => _onfootTargetPosition;
    public Vector3 OnfootTargetSpeed => _onfootTargetSpeed;
    public Vector3 IncarTargetPosition => _incarTargetPosition;
    public Vector3 IncarTargetSpeed => _incarTargetSpeed;
    public Vector3 PositionDifference => _positionDifference;
    public CRemotePlayerIncarTargetRotation IncarTargetRotation => _incarTargetRotation;
    public float ReportedArmour => _reportedArmour;
    public float ReportedHealth => _reportedHealth;
    public SampAnimation Animation => _animation;
    public byte UpdateType => _updateType;
    public uint LastUpdate => _lastUpdate;
    public uint LastTimestamp => _lastTimestamp;
    public bool PerformingCustomAnimation => _performingCustomAnimationRaw != 0;
    public CPlayerHeadState Head => _head;
    public bool MarkerState => _markerStateRaw != 0;
    public CRemotePlayerMarkerPosition MarkerPosition => _markerPosition;
    public int MarkerHandle => _markerHandle;

    public float GetDistanceToLocalPlayer() => _getDistanceToLocalPlayer((CRemotePlayer*)Unsafe.AsPointer(ref this));
    public void SetColor(uint colorRgba) => _setColor((CRemotePlayer*)Unsafe.AsPointer(ref this), colorRgba);
    public uint GetColorAsRgba() => _getColorAsRgba((CRemotePlayer*)Unsafe.AsPointer(ref this));
    public uint GetColorAsArgb() => _getColorAsArgb((CRemotePlayer*)Unsafe.AsPointer(ref this));
    public int GetStatus() => _getStatus((CRemotePlayer*)Unsafe.AsPointer(ref this));
    public bool DoesExist() => _doesExist((CRemotePlayer*)Unsafe.AsPointer(ref this)) != 0;
}
