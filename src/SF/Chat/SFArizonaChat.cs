using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Ui;

public sealed class SFArizonaChat : ISFArizonaChat
{
    private readonly Lock _sync = new();
    private readonly Dictionary<byte, SFArizonaChatRoomDef> _userRooms = new();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void HideDynamicRoomNative(byte roomId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ResetDynamicRoomsNative();

    private HideDynamicRoomNative? _hideDetour;
    private HideDynamicRoomNative? _hideOriginal;
    private ResetDynamicRoomsNative? _resetDetour;
    private ResetDynamicRoomsNative? _resetOriginal;
    private bool _hooksInstalled;

    public bool IsAvailable => CArizonaChat.IsAvailable;

    /// <summary>
    /// Registers a user-owned dynamic room. The room is created immediately via _chat.asi and
    /// will be automatically restored after server-triggered ResetDynamicRooms calls.
    /// Server HideDynamicRoom packets targeting this room ID will be blocked.
    /// Recommended room IDs: 200–254 to avoid collisions with server-managed rooms.
    /// </summary>
    public bool CreateRoom(byte roomId, string name, uint colorArgb, string icon = "", bool visible = true)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!CArizonaChat.IsAvailable)
            return false;

        EnsureHooks();

        lock (_sync)
        {
            _userRooms[roomId] = new SFArizonaChatRoomDef(roomId, name, colorArgb, icon, visible);
        }

        return CArizonaChat.TryUpsertRoom(roomId, name, colorArgb, icon, visible);
    }

    /// <summary>
    /// Removes a user-owned room from tracking and hides it from the chat UI.
    /// After removal, the room is no longer protected from server resets.
    /// </summary>
    public bool RemoveRoom(byte roomId)
    {
        lock (_sync)
        {
            if (!_userRooms.Remove(roomId))
                return false;
        }

        return CArizonaChat.TryHideRoom(roomId);
    }

    /// <summary>
    /// Writes a message to a user-owned dynamic room.
    /// The room must have been created via <see cref="CreateRoom"/> first.
    /// </summary>
    public bool WriteToRoom(byte roomId, string text, uint argbColor)
    {
        lock (_sync)
        {
            if (!_userRooms.ContainsKey(roomId))
                return false;
        }

        return CArizonaChat.TryAddToRoom(roomId, text, argbColor);
    }

    /// <summary>
    /// Returns all currently registered user-owned room definitions.
    /// </summary>
    public SFArizonaChatRoomDef[] GetUserRooms()
    {
        lock (_sync)
        {
            return [.. _userRooms.Values];
        }
    }

    /// <summary>
    /// Checks whether a room ID is registered as user-owned and therefore protected.
    /// </summary>
    public bool IsUserRoom(byte roomId)
    {
        lock (_sync)
        {
            return _userRooms.ContainsKey(roomId);
        }
    }

    private void EnsureHooks()
    {
        if (_hooksInstalled)
            return;

        lock (_sync)
        {
            if (_hooksInstalled)
                return;

            nint hideAddr = CArizonaChat.HideDynamicRoomAddress;
            nint resetAddr = CArizonaChat.ResetDynamicRoomsAddress;

            if (hideAddr != 0)
            {
                _hideDetour = new HideDynamicRoomNative(OnHideDynamicRoom);
                _hideOriginal = HookRuntime.Engine.CreateHook((IntPtr)hideAddr, _hideDetour);
                HookRuntime.Engine.EnableHook(_hideOriginal);
                SFLog.Debug($"SFArizonaChat: HideDynamicRoom hook installed at 0x{hideAddr:X8}.");
            }

            if (resetAddr != 0)
            {
                _resetDetour = new ResetDynamicRoomsNative(OnResetDynamicRooms);
                _resetOriginal = HookRuntime.Engine.CreateHook((IntPtr)resetAddr, _resetDetour);
                HookRuntime.Engine.EnableHook(_resetOriginal);
                SFLog.Debug($"SFArizonaChat: ResetDynamicRooms hook installed at 0x{resetAddr:X8}.");
            }

            _hooksInstalled = true;
        }
    }

    private void OnHideDynamicRoom(byte roomId)
    {
        if (IsUserRoom(roomId))
        {
            SFLog.Debug($"SFArizonaChat: Blocked HideDynamicRoom for user room {roomId}.");
            return;
        }

        _hideOriginal?.Invoke(roomId);
    }

    private int OnResetDynamicRooms()
    {
        int result = _resetOriginal?.Invoke() ?? 0;
        RestoreAllUserRooms();
        return result;
    }

    private void RestoreAllUserRooms()
    {
        SFArizonaChatRoomDef[] rooms;
        lock (_sync)
        {
            rooms = [.. _userRooms.Values];
        }

        foreach (SFArizonaChatRoomDef room in rooms)
        {
            CArizonaChat.TryUpsertRoom(room.RoomId, room.Name, room.ColorArgb, room.Icon, room.Visible);
        }

        if (rooms.Length > 0)
            SFLog.Debug($"SFArizonaChat: Restored {rooms.Length} user room(s) after reset.");
    }
}
