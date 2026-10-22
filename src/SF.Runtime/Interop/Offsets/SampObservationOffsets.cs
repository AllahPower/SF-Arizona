namespace SFSharp.Runtime.Interop.Offsets;

internal enum SampObservationPoint
{
    Connect, PublicKeyMismatch, Banned, RequestAccepted, ServerFull, Disconnected,
    InvalidPassword, AttemptFailed, ConnectionLost, JoinHandshake,
    InitializePools, ResetPools, ResetPlayers, ResetVehicles, ResetObjects, ResetActors,
    ResetPickups, ResetMenus, ResetTextDraws, ResetGangZones, ResetLabels,
    RemoteProcess, RemoteOnFoot, RemoteVehicle, RemotePassenger, RemoteAim, RemoteTrailer,
    RemoteEnterVehicle, RemoteExitVehicle, RemoteKill,
    SelectionEnable, SelectionDisable, SelectionNotify, SelectionProcess,
    PickupCreate, PickupDelete, PickupNotify,
    SpectatingEnable, SpectatingPlayer, SpectatingVehicle,
}

/// <summary>Pure revision table for observer bindings. Zero means unverified, never a fallback address.</summary>
/// <remarks>SAMP-API multiver 6d4db99. R5 pickup Create and reset labels corrected from the audited IDB.
/// R1 Connect has no binding in this upstream revision; other R1 session handlers are available.</remarks>
internal static class SampObservationOffsets
{
    internal static int Resolve(SFSampVersion version, SampObservationPoint point)
    {
        (int r1, int r3, int r5) = point switch
        {
            SampObservationPoint.Connect => (0, 0x85D0, 0x8940),
            SampObservationPoint.PublicKeyMismatch => (0x8850, 0x89E0, 0x8D50),
            SampObservationPoint.Banned => (0x8870, 0x8A00, 0x8D70),
            SampObservationPoint.RequestAccepted => (0x8890, 0x8A20, 0x8D90),
            SampObservationPoint.ServerFull => (0x88B0, 0x8A40, 0x8DB0),
            SampObservationPoint.Disconnected => (0x88E0, 0x8A70, 0x8DE0),
            SampObservationPoint.InvalidPassword => (0x8920, 0x8AB0, 0x8E20),
            SampObservationPoint.AttemptFailed => (0x8960, 0x8AF0, 0x8E60),
            SampObservationPoint.ConnectionLost => (0xA800, 0xA990, 0xACF0),
            SampObservationPoint.JoinHandshake => (0xA890, 0xAA20, 0xAD80),
            SampObservationPoint.InitializePools => (0x8160, 0x81D0, 0x8540),
            SampObservationPoint.ResetPools => (0xA010, 0xA190, 0xA4F0),
            SampObservationPoint.ResetPlayers => (0x8AE0, 0x8C70, 0x8FE0),
            SampObservationPoint.ResetVehicles => (0x8B80, 0x8D10, 0x9080),
            SampObservationPoint.ResetObjects => (0x8CC0, 0x8E50, 0x91B0),
            SampObservationPoint.ResetActors => (0x8FA0, 0x9120, 0x9490),
            SampObservationPoint.ResetPickups => (0x8E00, 0x8F90, 0x92F0),
            SampObservationPoint.ResetMenus => (0x8E60, 0x8FF0, 0x9350),
            SampObservationPoint.ResetTextDraws => (0x8C20, 0x8DB0, 0x9110),
            SampObservationPoint.ResetGangZones => (0x8D60, 0x8EF0, 0x9250),
            SampObservationPoint.ResetLabels => (0x8F00, 0x9080, 0x93F0),
            SampObservationPoint.RemoteProcess => (0x12EF0, 0x16110, 0x166B0),
            SampObservationPoint.RemoteOnFoot => (0x139A0, 0x16BB0, 0x17260),
            SampObservationPoint.RemoteVehicle => (0x13A80, 0x16C90, 0x17340),
            SampObservationPoint.RemotePassenger => (0x13B70, 0x16D80, 0x17440),
            SampObservationPoint.RemoteAim => (0x12080, 0x15230, 0x15760),
            SampObservationPoint.RemoteTrailer => (0x12520, 0x15740, 0x15C90),
            SampObservationPoint.RemoteEnterVehicle => (0x12A20, 0x15C30, 0x161A0),
            SampObservationPoint.RemoteExitVehicle => (0x12AB0, 0x15CC0, 0x16230),
            SampObservationPoint.RemoteKill => (0x13CA0, 0x16EB0, 0x17570),
            SampObservationPoint.SelectionEnable => (0x6CE60, 0x70D50, 0x71440),
            SampObservationPoint.SelectionDisable => (0x6CF40, 0x70E30, 0x71520),
            SampObservationPoint.SelectionNotify => (0x6CEA0, 0x70D90, 0x71480),
            SampObservationPoint.SelectionProcess => (0x6CD30, 0x70C20, 0x71310),
            SampObservationPoint.PickupCreate => (0xFDC0, 0x12F20, 0x13270),
            SampObservationPoint.PickupDelete => (0xFE70, 0x12FD0, 0x13320),
            SampObservationPoint.PickupNotify => (0xFF90, 0x130F0, 0x13440),
            SampObservationPoint.SpectatingEnable => (0x4000, 0x4010, 0x41C0),
            SampObservationPoint.SpectatingPlayer => (0x40B0, 0x40D0, 0x4280),
            SampObservationPoint.SpectatingVehicle => (0x4060, 0x4080, 0x4230),
            _ => throw new ArgumentOutOfRangeException(nameof(point)),
        };
        int rva = version switch { SFSampVersion.R1 => r1, SFSampVersion.R3_1 => r3, SFSampVersion.R5_1 => r5, _ => 0 };
        return rva != 0 ? rva : throw new SFSampFeatureUnavailableException(version, point.ToString());
    }
}
