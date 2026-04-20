using System.Runtime.CompilerServices;
using System.Text;

namespace SFSharp.Runtime.Interop.Classes;

using unsafe ArizonaChatAddEntryDelegate = delegate* unmanaged[Stdcall]<int, byte*, byte*, uint, uint, int>;
// cdecl(_DWORD* output, int* pColor, byte** pText, byte* pFlags) -> _DWORD*
using unsafe CreateFromRawEntryArgsDelegate = delegate* unmanaged[Cdecl]<nint*, nint*, byte**, byte*, nint*>;
// cdecl(_DWORD* msgHandle, byte chatType, byte flags) -> int
using unsafe DispatchMessageToHandlersDelegate = delegate* unmanaged[Cdecl]<nint*, byte, byte, int>;
using unsafe HideDynamicRoomDelegate = delegate* unmanaged[Cdecl]<byte, void>;
// thiscall(refcountObj) -> int
using unsafe RefCountedReleaseDelegate = delegate* unmanaged[Thiscall]<nint, int>;
using unsafe ResetDynamicRoomsDelegate = delegate* unmanaged[Cdecl]<int>;
using unsafe UpsertDynamicRoomDelegate = delegate* unmanaged[Cdecl]<byte, byte*, nuint, int, byte*, nuint, int, byte>;

public enum ArizonaChatRoomType
{
    Unknown = 0,
    Chat = 1,
    Find = 2,
    Regex = 3,
    Proxy = 4,
    Udp = 5,
    Server = 6
}

public readonly record struct ArizonaChatRoomSnapshot(
    int RegistryIndex,
    int RoomIndex,
    ArizonaChatRoomType Type,
    string Name,
    uint ColorArgb,
    bool IsActive,
    bool IsHiddenByConfig,
    bool IsInactive,
    bool HasMuteOverride,
    bool IsMuted,
    bool IsDynamic,
    byte? RoomId,
    string? IconToken);

public delegate void ArizonaChatRoomVisitor(in ArizonaChatRoomSnapshot room);

public static unsafe class CArizonaChat
{
    private const string ModuleName = "_chat.asi";
    private static readonly Encoding RoomStringEncoding = CreateRoomStringEncoding();
    private static readonly UTF8Encoding Utf8Strict = new(false, true);
    private const int MaxRoomStringBytes = 260;

    // Exact RVAs below were confirmed against the current Arizona _chat.asi build.
    // If Arizona updates _chat.asi and strips our names/comments in IDA, restore them from these use-sites:
    // 1. ScreenChat_Initialize:
    //    - after chat_rooms.json/chat_rooms_user.json are loaded, it iterates the general room registry
    //    - look for writes into the dynamic fast-map table for type == 6 rooms
    // 2. ScreenChat_UpdateRoomSelectorUi:
    //    - walks the general room registry using buckets/base/count/active index
    //    - this is the best anchor for ActiveRoomIndex + RoomRegistry{Buckets,Mask,BaseIndex,Count}
    // 3. ScreenChat_DispatchToRoomHandler / ScreenChatRoomRegistry_{Upsert,Hide,Reset}DynamicRoom:
    //    - these anchor the dynamic room fast-map globals {Buckets,Mask,BaseId,Count}
    // Important: _chat.asi stores bucket count in the "Mask" globals, and then uses (bucketCount - 1) as the
    // actual bitmask before AND. Do not treat these values as a ready-to-use mask.
    private static class RoomRuntimeRva
    {
        public const uint ActiveRoomIndex = 0x955CC;
        public const uint DynamicRoomSlotTableBuckets = 0x955F8;
        public const uint DynamicRoomSlotTableMask = 0x955FC;
        public const uint DynamicRoomBaseId = 0x95600;
        public const uint DynamicRoomCount = 0x95604;
        public const uint RoomRegistryBuckets = 0x956B8;
        public const uint RoomRegistryMask = 0x956BC;
        public const uint RoomRegistryBaseIndex = 0x956C0;
        public const uint RoomRegistryCount = 0x956C4;
    }

    private static class RoomOffsets
    {
        // Restored from room object constructors and selector/runtime use-sites:
        // - ScreenChatRoomConfig_LoadFromJson
        // - ScreenChat_UpdateRoomSelectorUi
        // - ScreenChat_Initialize dynamic room mapping loop
        public const int Type = 0x04;
        public const int Name = 0x08;
        public const int Color = 0x20;
        public const int HasMuteOverride = 0x29;
        public const int Mute = 0x2A;
        public const int Flags = 0x2B;
        public const int DynamicRoomId = 0x4C;
        public const int DynamicIconToken = 0x50;
    }

    private static class SmallStringOffsets
    {
        // Arizona _chat.asi uses the same small-string layout repeatedly across room names and tokens:
        // [0x00] inline chars or heap ptr, [0x14] capacity, inline capacity = 15.
        // Reconfirm from any room/string copy helper if the module is updated.
        public const int Data = 0x00;
        public const int Capacity = 0x14;
        public const int InlineCapacity = 15;
    }

    // ChatHooks_OnSampAddEntry — stdcall thunk that forwards into ScreenChat_AddFormattedEntry
    private static readonly byte?[] AddEntryThunkPattern =
    [
        0x55, 0x8B, 0xEC, 0xFF, 0x75, 0x18, 0xFF, 0x75, 0x14,
        0xFF, 0x75, 0x10, 0xFF, 0x75, 0x0C, 0xFF, 0x75, 0x08,
        0xE8, null, null, null, null, 0x83, 0xC4, 0x14, 0x5D,
        0xC2, 0x14, 0x00
    ];

    // ScreenChatRoomRegistry_UpsertDynamicRoom — cdecl(byte roomId, char* name, size_t nameLen, int color, char* icon, size_t iconLen, int visible)
    private static readonly byte?[] UpsertDynamicRoomPattern =
    [
        0x6A, 0x04, 0xB8, null, null, null, null, 0xE8, null, null, null, null,
        0x0F, 0xB6, 0x7D, 0x08, 0x3B, 0x3D, null, null, null, null,
        0x72, null, 0x8D, 0x47, 0x01
    ];

    // ScreenChatRoomRegistry_HideDynamicRoom — cdecl(byte roomId)
    private static readonly byte?[] HideDynamicRoomPattern =
    [
        0x55, 0x8B, 0xEC, 0x56, 0x0F, 0xB6, 0x75, 0x08,
        0x3B, 0x35, null, null, null, null, 0x73, null,
        0x8B, 0x15, null, null, null, null, 0xA1
    ];

    // ScreenChatRoomRegistry_ResetDynamicRooms — cdecl()
    private static readonly byte?[] ResetDynamicRoomsPattern =
    [
        0x56, 0x8B, 0x35, null, null, null, null, 0x57,
        0x8B, 0x3D, null, null, null, null, 0x03, 0xFE,
        0xEB, null, 0xA1, null, null, null, null, 0x8B, 0xCE
    ];

    // ScreenChat_AddFormattedEntry — cdecl(int type, char* text, char prefix, int textColor, ...)
    // Used to extract CreateFromRawEntryArgs address at call-site offset +0xAA.
    private static readonly byte?[] AddFormattedEntryPattern =
    [
        0x68, 0x20, 0x01, 0x00, 0x00, 0xB8, null, null, null, null,
        0xE8, null, null, null, null, 0x8B, 0x45, 0x0C, 0x33, 0xF6,
        0x8B, 0x4D, 0x10
    ];

    // ScreenChat_DispatchMessageToHandlers — cdecl(_DWORD* msgHandle, byte chatType, byte flags)
    private static readonly byte?[] DispatchMessageToHandlersPattern =
    [
        0x55, 0x8B, 0xEC, 0x57, 0x8B, 0x7D, 0x08, 0xFF, 0x37,
        0xE8, null, null, null, null, 0x80, 0x7D, 0x0C, 0x00, 0x59
    ];

    // RefCounted_Release — thiscall(this)
    private static readonly byte?[] RefCountedReleasePattern =
    [
        0x56, 0x57, 0x83, 0xCF, 0xFF, 0x8B, 0xF1, 0x8B, 0xC7,
        0xF0, 0x0F, 0xC1, 0x46, 0x04, 0x75, null, 0x8B, 0x06,
        0xFF, 0x10, 0xF0, 0x0F, 0xC1, 0x7E, 0x08
    ];

    private static nint _addEntryThunk;
    private static nint _upsertDynamicRoom;
    private static nint _hideDynamicRoom;
    private static nint _resetDynamicRooms;
    private static nint _createFromRawEntryArgs;
    private static nint _dispatchMessageToHandlers;
    private static nint _refCountedRelease;
    private static bool _resolved;

    public static bool IsAvailable => ResolveAll() && _addEntryThunk != 0;
    public static bool AreRoomsAvailable => TryReadRoomRegistryHeader(out _);

    internal static nint HideDynamicRoomAddress { get { ResolveAll(); return _hideDynamicRoom; } }
    internal static nint ResetDynamicRoomsAddress { get { ResolveAll(); return _resetDynamicRooms; } }

    public static bool TryAddEntry(EntryType type, string? text, string? prefix, uint textColor, uint prefixColor)
    {
        if (!ResolveAll() || _addEntryThunk == 0)
        {
            return false;
        }

        using AnsiString textAnsi = AnsiString.Encode(text);
        using AnsiString prefixAnsi = AnsiString.Encode(prefix);
        _ = ((ArizonaChatAddEntryDelegate)_addEntryThunk)((int)type, textAnsi, prefixAnsi, textColor, prefixColor);
        return true;
    }

    /// <summary>
    /// Creates or updates a dynamic Arizona ScreenChat room.
    /// Important runtime behavior:
    /// 1. _chat.asi can soft-hide or reset dynamic rooms after connect/reconnect.
    /// 2. After reconnect, the room may disappear even if no explicit HideDynamicRoom packet was observed.
    /// 3. To keep a custom room alive, callers should recreate it after the post-connect chat bootstrap wave.
    /// 4. Do not rely on a low room id because the server owns those ids and can overwrite them with SetChatGroup.
    /// </summary>
    public static bool TryUpsertRoom(byte roomId, string name, uint color, string? icon, bool visible)
    {
        if (!ResolveAll() || _upsertDynamicRoom == 0)
        {
            return false;
        }

        using AnsiString nameAnsi = AnsiString.Encode(name);
        using AnsiString iconAnsi = AnsiString.Encode(icon ?? string.Empty);
        byte* namePtr = nameAnsi;
        byte* iconPtr = iconAnsi;
        nuint nameLen = namePtr != null ? (nuint)NativeStrLen(namePtr) : 0;
        nuint iconLen = iconPtr != null ? (nuint)NativeStrLen(iconPtr) : 0;
        _ = ((UpsertDynamicRoomDelegate)_upsertDynamicRoom)(
            roomId, namePtr, nameLen, (int)color, iconPtr, iconLen, visible ? 1 : 0);
        return true;
    }

    public static bool TryHideRoom(byte roomId)
    {
        if (!ResolveAll() || _hideDynamicRoom == 0)
        {
            return false;
        }

        ((HideDynamicRoomDelegate)_hideDynamicRoom)(roomId);
        return true;
    }

    public static bool TryResetRooms()
    {
        if (!ResolveAll() || _resetDynamicRooms == 0)
        {
            return false;
        }

        _ = ((ResetDynamicRoomsDelegate)_resetDynamicRooms)();
        return true;
    }

    /// <summary>
    /// Creates a ScreenChat message and dispatches it to a specific dynamic room.
    /// The room must exist and still be active at the _chat.asi runtime layer.
    /// Notes for callers:
    /// 1. Writing to a room does not recreate it automatically if the room was hidden or reset.
    /// 2. If the server or reconnect flow hides the room, call TryUpsertRoom again before writing.
    /// 3. In practice, the safest flow is reserve a custom room-id range and recreate the room after connect/reconnect.
    /// </summary>
    public static bool TryAddToRoom(byte roomId, string text, uint argbColor)
    {
        if (!ResolveAll()
            || _createFromRawEntryArgs == 0
            || _dispatchMessageToHandlers == 0
            || _refCountedRelease == 0)
        {
            return false;
        }

        using AnsiString textAnsi = AnsiString.Encode(text);
        byte* textPtr = textAnsi;
        if (textPtr == null)
        {
            return false;
        }

        // CreateFromRawEntryArgs(output, &color, &textPtr, &flags)
        //   output[0] = obj+16 (payload ptr)
        //   output[1] = obj    (refcounted wrapper, initial refcount=1)
        nint* output = stackalloc nint[2];
        output[0] = 0;
        output[1] = 0;
        nint colorValue = (nint)argbColor;
        byte flags = 1;

        ((CreateFromRawEntryArgsDelegate)_createFromRawEntryArgs)(output, &colorValue, &textPtr, &flags);

        if (output[1] == 0)
        {
            return false;
        }

        // Dispatch to room handler with chatType = roomId
        // Message handle layout: [0]=payload_ptr, [1]=refcount_obj
        nint* msgHandle = stackalloc nint[2];
        msgHandle[0] = output[0];
        msgHandle[1] = output[1];

        ((DispatchMessageToHandlersDelegate)_dispatchMessageToHandlers)(msgHandle, roomId, 0);

        // Release our reference (refcount 1→0 triggers destruction)
        ((RefCountedReleaseDelegate)_refCountedRelease)(output[1]);

        return true;
    }

    /// <summary>
    /// Reads a safe snapshot of all visible ScreenChat room objects from the general room registry.
    /// Callers should treat the returned records as ephemeral snapshots and never cache native pointers.
    /// For safety this method only succeeds on the game main thread.
    /// </summary>
    public static bool TryGetRooms(out ArizonaChatRoomSnapshot[] rooms)
    {
        rooms = [];
        if (!TryReadRoomRegistryHeader(out ArizonaChatRoomRegistryHeader header))
        {
            return false;
        }

        List<ArizonaChatRoomSnapshot> list = new(header.Count);
        if (!TryEnumerateRoomsInternal(header, static (in ArizonaChatRoomSnapshot room, List<ArizonaChatRoomSnapshot> state) =>
            {
                state.Add(room);
            }, list))
        {
            return false;
        }

        rooms = [.. list];
        return true;
    }

    public static ArizonaChatRoomSnapshot[] GetRooms()
    {
        return TryGetRooms(out ArizonaChatRoomSnapshot[] rooms) ? rooms : [];
    }

    public static bool TryGetActiveRoom(out ArizonaChatRoomSnapshot room)
    {
        room = default;
        if (!TryReadRoomRegistryHeader(out ArizonaChatRoomRegistryHeader header))
        {
            return false;
        }

        bool found = false;
        _ = TryEnumerateRoomsInternal(header, static (in ArizonaChatRoomSnapshot current, StrongBox<ArizonaChatRoomSnapshot> state) =>
        {
            if (current.IsActive)
            {
                state.Value = current;
            }
        }, new StrongBox<ArizonaChatRoomSnapshot>(), ref found, requireActiveMatch: true, out room);

        return found;
    }

    public static bool TryEnumerateRooms(ArizonaChatRoomVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        if (!TryReadRoomRegistryHeader(out ArizonaChatRoomRegistryHeader header))
        {
            return false;
        }

        return TryEnumerateRoomsInternal(header, static (in ArizonaChatRoomSnapshot room, ArizonaChatRoomVisitor callback) =>
        {
            callback(room);
        }, visitor);
    }

    private static bool ResolveAll()
    {
        if (_resolved)
        {
            return true;
        }

        if (!ModuleResolver.IsModuleLoaded(ModuleName))
        {
            return false;
        }

        _addEntryThunk = ModuleResolver.FindPattern(ModuleName, AddEntryThunkPattern);
        _upsertDynamicRoom = ModuleResolver.FindPattern(ModuleName, UpsertDynamicRoomPattern);
        _hideDynamicRoom = ModuleResolver.FindPattern(ModuleName, HideDynamicRoomPattern);
        _resetDynamicRooms = ModuleResolver.FindPattern(ModuleName, ResetDynamicRoomsPattern);
        _dispatchMessageToHandlers = ModuleResolver.FindPattern(ModuleName, DispatchMessageToHandlersPattern);
        _refCountedRelease = ModuleResolver.FindPattern(ModuleName, RefCountedReleasePattern);

        // Resolve CreateFromRawEntryArgs by reading the E8 relative call inside AddFormattedEntry at offset +0xAA.
        nint addFormattedEntry = ModuleResolver.FindPattern(ModuleName, AddFormattedEntryPattern);
        if (addFormattedEntry != 0)
        {
            nint callSite = addFormattedEntry + 0xAA;
            if (*(byte*)callSite == 0xE8)
            {
                int relOffset = *(int*)(callSite + 1);
                _createFromRawEntryArgs = callSite + 5 + relOffset;
            }

            SFLog.Debug($"Resolved Arizona chat AddFormattedEntry at 0x{addFormattedEntry:X8}.");
        }

        if (_addEntryThunk == 0)
            SFLog.Warn("Arizona _chat.asi: AddEntry thunk pattern not found. Falling back to samp.dll CChat.");
        else
            SFLog.Debug($"Resolved Arizona chat AddEntry thunk at 0x{_addEntryThunk:X8}.");

        if (_upsertDynamicRoom != 0)
            SFLog.Debug($"Resolved Arizona chat UpsertDynamicRoom at 0x{_upsertDynamicRoom:X8}.");

        if (_hideDynamicRoom != 0)
            SFLog.Debug($"Resolved Arizona chat HideDynamicRoom at 0x{_hideDynamicRoom:X8}.");

        if (_resetDynamicRooms != 0)
            SFLog.Debug($"Resolved Arizona chat ResetDynamicRooms at 0x{_resetDynamicRooms:X8}.");

        if (_createFromRawEntryArgs != 0)
            SFLog.Debug($"Resolved Arizona chat CreateFromRawEntryArgs at 0x{_createFromRawEntryArgs:X8}.");

        if (_dispatchMessageToHandlers != 0)
            SFLog.Debug($"Resolved Arizona chat DispatchMessageToHandlers at 0x{_dispatchMessageToHandlers:X8}.");

        if (_refCountedRelease != 0)
            SFLog.Debug($"Resolved Arizona chat RefCounted_Release at 0x{_refCountedRelease:X8}.");

        _resolved = true;
        return true;
    }

    private static int NativeStrLen(byte* str)
    {
        int len = 0;
        while (str[len] != 0) len++;
        return len;
    }

    private static bool TryReadRoomRegistryHeader(out ArizonaChatRoomRegistryHeader header)
    {
        header = default;
        if (SynchronizationContext.Current is not SFSynchronizationContext)
        {
            return false;
        }

        if (!TryGetModuleBase(out nint moduleBase))
        {
            return false;
        }

        if (!TryReadInt32(moduleBase + (nint)RoomRuntimeRva.RoomRegistryCount, out int count)
            || count <= 0
            || count > 512
            || !TryReadInt32(moduleBase + (nint)RoomRuntimeRva.RoomRegistryBaseIndex, out int baseIndex)
            || !TryReadInt32(moduleBase + (nint)RoomRuntimeRva.RoomRegistryMask, out int bucketCount)
            || bucketCount <= 0
            || bucketCount > 0x10000
            || !TryReadPointer(moduleBase + (nint)RoomRuntimeRva.RoomRegistryBuckets, out nint buckets)
            || buckets == 0
            || !TryReadInt32(moduleBase + (nint)RoomRuntimeRva.ActiveRoomIndex, out int activeRoomIndex))
        {
            return false;
        }

        header = new(moduleBase, buckets, bucketCount, baseIndex, count, activeRoomIndex);
        return true;
    }

    private static bool TryEnumerateRoomsInternal<TState>(
        ArizonaChatRoomRegistryHeader header,
        RoomSnapshotAction<TState> action,
        TState state)
    {
        bool ignoredFound = false;
        return TryEnumerateRoomsInternal(header, action, state, ref ignoredFound, requireActiveMatch: false, out _);
    }

    private static bool TryEnumerateRoomsInternal<TState>(
        ArizonaChatRoomRegistryHeader header,
        RoomSnapshotAction<TState> action,
        TState state,
        ref bool found,
        bool requireActiveMatch,
        out ArizonaChatRoomSnapshot activeRoom)
    {
        activeRoom = default;

        for (int roomIndex = 0; roomIndex < header.Count; roomIndex++)
        {
            int registryIndex = header.BaseIndex + roomIndex;
            if (!TryReadRoomPointer(header, registryIndex, out nint roomPtr) || roomPtr == 0)
            {
                continue;
            }

            if (!TryReadRoomSnapshot(roomPtr, registryIndex, roomIndex, header.ActiveRoomIndex == roomIndex, out ArizonaChatRoomSnapshot room))
            {
                continue;
            }

            if (room.IsActive)
            {
                activeRoom = room;
                found = true;
            }

            action(room, state);
        }

        return !requireActiveMatch || found;
    }

    private static bool TryReadRoomPointer(ArizonaChatRoomRegistryHeader header, int registryIndex, out nint roomPtr)
    {
        roomPtr = 0;
        // In _chat.asi code this is always:
        // bucketIndex = (registryIndex >> 2) & (bucketCount - 1)
        // bucketSlot  = buckets[bucketIndex]
        // roomPtr     = bucketSlot[registryIndex & 3]
        int bucketMask = header.BucketCount - 1;
        int bucketIndex = (registryIndex >> 2) & bucketMask;
        nint bucketSlotAddress = header.Buckets + bucketIndex * sizeof(nint);
        if (!TryReadPointer(bucketSlotAddress, out nint bucketPtr) || bucketPtr == 0)
        {
            return false;
        }

        nint roomSlotAddress = bucketPtr + (registryIndex & 3) * sizeof(nint);
        return TryReadPointer(roomSlotAddress, out roomPtr);
    }

    private static bool TryReadRoomSnapshot(nint roomPtr, int registryIndex, int roomIndex, bool isActive, out ArizonaChatRoomSnapshot room)
    {
        room = default;
        if (!NativeMemoryValidator.IsReadable(roomPtr, 0x54))
        {
            return false;
        }

        if (!TryReadInt32(roomPtr + RoomOffsets.Type, out int roomTypeValue))
        {
            return false;
        }

        ArizonaChatRoomType type = Enum.IsDefined(typeof(ArizonaChatRoomType), roomTypeValue)
            ? (ArizonaChatRoomType)roomTypeValue
            : ArizonaChatRoomType.Unknown;

        if (!TryReadAnsiSmallString(roomPtr + RoomOffsets.Name, out string name))
        {
            name = string.Empty;
        }

        _ = TryReadUInt32(roomPtr + RoomOffsets.Color, out uint colorArgb);
        bool hasMuteOverride = TryReadByte(roomPtr + RoomOffsets.HasMuteOverride, out byte hasMuteOverrideByte) && hasMuteOverrideByte != 0;
        bool isMuted = TryReadByte(roomPtr + RoomOffsets.Mute, out byte muteByte) && muteByte != 0;

        byte flags = 0;
        _ = TryReadByte(roomPtr + RoomOffsets.Flags, out flags);

        byte? roomId = null;
        string? iconToken = null;
        bool isDynamic = type == ArizonaChatRoomType.Server;
        if (isDynamic)
        {
            if (TryReadByte(roomPtr + RoomOffsets.DynamicRoomId, out byte dynamicRoomId))
            {
                roomId = dynamicRoomId;
            }

            if (TryReadAnsiSmallString(roomPtr + RoomOffsets.DynamicIconToken, out string dynamicIcon))
            {
                iconToken = dynamicIcon;
            }
        }

        room = new(
            registryIndex,
            roomIndex,
            type,
            name,
            colorArgb,
            isActive,
            (flags & 1) != 0,
            (flags & 2) != 0,
            hasMuteOverride,
            isMuted,
            isDynamic,
            roomId,
            iconToken);
        return true;
    }

    private static bool TryReadAnsiSmallString(nint stringAddress, out string value)
    {
        value = string.Empty;
        if (!NativeMemoryValidator.IsReadable(stringAddress, 0x18))
        {
            return false;
        }

        if (!TryReadInt32(stringAddress + SmallStringOffsets.Capacity, out int capacity))
        {
            return false;
        }

        nint sourceAddress = stringAddress + SmallStringOffsets.Data;
        if (capacity > SmallStringOffsets.InlineCapacity)
        {
            if (!TryReadPointer(sourceAddress, out sourceAddress) || sourceAddress == 0)
            {
                return false;
            }
        }

        return TryReadAnsiCString(sourceAddress, MaxRoomStringBytes, out value);
    }

    private static bool TryReadAnsiCString(nint address, int maxBytes, out string value)
    {
        value = string.Empty;
        if (!NativeMemoryValidator.IsReadable(address, 1))
        {
            return false;
        }

        List<byte> bytes = [];
        for (int i = 0; i < maxBytes; i++)
        {
            nint cursor = address + i;
            if (!NativeMemoryValidator.IsReadable(cursor, 1))
            {
                break;
            }

            byte current = *(byte*)cursor;
            if (current == 0)
            {
                break;
            }

            bytes.Add(current);
        }

        value = bytes.Count == 0 ? string.Empty : DecodeRoomString([.. bytes]);
        return true;
    }

    private static bool TryReadPointer(nint address, out nint value)
    {
        value = 0;
        if (!NativeMemoryValidator.IsReadable(address, (nuint)sizeof(nint)))
        {
            return false;
        }

        value = *(nint*)address;
        return true;
    }

    private static bool TryReadInt32(nint address, out int value)
    {
        value = 0;
        if (!NativeMemoryValidator.IsReadable(address, sizeof(int)))
        {
            return false;
        }

        value = *(int*)address;
        return true;
    }

    private static bool TryReadUInt32(nint address, out uint value)
    {
        value = 0;
        if (!NativeMemoryValidator.IsReadable(address, sizeof(uint)))
        {
            return false;
        }

        value = *(uint*)address;
        return true;
    }

    private static bool TryReadByte(nint address, out byte value)
    {
        value = 0;
        if (!NativeMemoryValidator.IsReadable(address, sizeof(byte)))
        {
            return false;
        }

        value = *(byte*)address;
        return true;
    }

    private static bool TryGetModuleBase(out nint moduleBase)
    {
        moduleBase = (nint)Win32.GetModuleHandle(ModuleName);
        return moduleBase != 0;
    }

    private static Encoding CreateRoomStringEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1251);
    }

    private static string DecodeRoomString(byte[] bytes)
    {
        try
        {
            return Utf8Strict.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return RoomStringEncoding.GetString(bytes);
        }
    }

    private readonly record struct ArizonaChatRoomRegistryHeader(
        nint ModuleBase,
        nint Buckets,
        int BucketCount,
        int BaseIndex,
        int Count,
        int ActiveRoomIndex);

    private delegate void RoomSnapshotAction<in TState>(in ArizonaChatRoomSnapshot room, TState state);
}
