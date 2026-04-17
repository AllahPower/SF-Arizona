namespace SFSharp.Abstractions.Arizona;

/// <summary>
/// Plugin-facing Arizona ScreenChat facade for user-owned dynamic rooms. The host owns the native
/// hooks and room restoration logic; plugins interact only through this high-level contract.
/// </summary>
/// <remarks>NOT thread-safe. Drives native Arizona chat state - main-thread only.</remarks>
public interface ISFArizonaChat
{
    bool IsAvailable { get; }

    bool CreateRoom(byte roomId, string name, uint colorArgb, string icon = "", bool visible = true);
    bool RemoveRoom(byte roomId);
    bool WriteToRoom(byte roomId, string text, uint argbColor);

    SFArizonaChatRoomDef[] GetUserRooms();
    bool IsUserRoom(byte roomId);
}
