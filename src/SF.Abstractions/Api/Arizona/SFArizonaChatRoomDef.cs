namespace SFSharp.Abstractions.Arizona;

/// <summary>Definition of a user-owned Arizona ScreenChat room tracked by the host.</summary>
public readonly record struct SFArizonaChatRoomDef(
    byte RoomId,
    string Name,
    uint ColorArgb,
    string Icon,
    bool Visible);
