using System;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

using unsafe AddChatMessageDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CChat*, byte*, uint, byte*, void>;
using unsafe AddEntryDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CChat*, int, byte*, byte*, uint, uint, void>;
using unsafe AddMessageDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CChat*, uint, byte*, void>;


[StructLayout(LayoutKind.Explicit, Size = 25622, Pack = 1)]
public unsafe ref struct CChat
{
    private static readonly nuint _instanceAddress = (nuint)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.Instance);
    private static CChat* CurrentInstance => *(CChat**)_instanceAddress;
    public static ref readonly CChat Instance => ref *RequireInstance();

    public static bool IsAvailable
    {
        get
        {
            CChat* instance = CurrentInstance;
            return instance is not null
                && NativeMemoryValidator.IsReadable((nint)instance, (nuint)SampOffsets.CChat.Size);
        }
    }

    private static readonly AddEntryDelegate _addEntry = (AddEntryDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddEntry);
    private static readonly AddChatMessageDelegate _addChatMessage = (AddChatMessageDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddChatMessage);
    private static readonly AddMessageDelegate _addMessage = (AddMessageDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddMessage);

    // Arizona replaces the visible chat pipeline with _chat.asi ScreenChat.
    // When it is available, routing AddEntry through _chat.asi is both more correct and cheaper than
    // calling samp.dll first and letting Arizona reprocess the message through its own hooks/runtime.
    public void AddEntry(EntryType type, string? text, string? prefix, uint textColor, uint prefixColor)
    {
        if (CArizonaChat.TryAddEntry(type, text, prefix, textColor, prefixColor))
            return;

        CChat* instance = RequireInstance();
        using AnsiString textAnsi = AnsiString.Encode(text);
        using AnsiString prefixAnsi = AnsiString.Encode(prefix);
        _addEntry(instance, (int)type, textAnsi, prefixAnsi, textColor, prefixColor);
    }

    public void AddChatMessage(string? prefix, uint prefixColor, string? text)
    {
        CChat* instance = RequireInstance();
        using AnsiString prefixAnsi = AnsiString.Encode(prefix);
        using AnsiString textAnsi = AnsiString.Encode(text);
        _addChatMessage(instance, prefixAnsi, prefixColor, textAnsi);
    }

    public void AddMessage(uint color, string? text)
    {
        CChat* instance = RequireInstance();
        using AnsiString textAnsi = AnsiString.Encode(text);
        _addMessage(instance, color, textAnsi);
    }

    // -- Direct memory read --
    // These APIs read the original samp.dll chat history buffer directly.
    // They stop reflecting the visible chat as soon as another module replaces or decouples the runtime
    // from the original CChat storage.
    [Obsolete("Reads raw samp.dll chat buffer memory. Visible chat may be owned by a custom module instead of the original CChat buffer.")]
    public static NativeChatEntry ReadEntry(int index)
    {
        if ((uint)index >= SampOffsets.CChat.EntryCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        byte* entry = GetEntryPtr(RequireInstance(), index);
        return ParseEntry(entry);
    }

    [Obsolete("Reads raw samp.dll chat buffer memory. Visible chat may be owned by a custom module instead of the original CChat buffer.")]
    public static int ReadEntries(Span<NativeChatEntry> buffer)
    {
        CChat* instance = RequireInstance();
        int count = Math.Min(buffer.Length, SampOffsets.CChat.EntryCount);
        for (int i = 0; i < count; i++)
        {
            buffer[i] = ParseEntry(GetEntryPtr(instance, i));
        }

        return count;
    }

    // -- Direct memory write (game thread only, bypasses native AddEntry/Log) --
    // This raw write path only works while no external modification replaces the original samp.dll chat buffer.
    // If a custom chat runtime owns rendering and dispatch, the buffer can still change while nothing appears on screen.
    [Obsolete("Writes raw samp.dll chat buffer memory. Prefer AddEntry/AddMessage or a custom chat runtime when present.")]
    public static void DirectWriteEntry(EntryType type, string? text, string? prefix, uint textColor, uint prefixColor)
    {
        CChat* instance = RequireInstance();
        byte* entriesBase = (byte*)instance + SampOffsets.CChat.Entries;
        int entrySize = SampOffsets.CChat.EntrySize;

        // Shift entries[1..99] → entries[0..98] (same as native PushBack)
        Buffer.MemoryCopy(
            source: entriesBase + entrySize,
            destination: entriesBase,
            destinationSizeInBytes: 99 * entrySize,
            sourceBytesToCopy: 99 * entrySize);

        // Write to slot 99
        byte* slot = entriesBase + 99 * entrySize;
        new Span<byte>(slot, entrySize).Clear();

        *(int*)(slot + SampOffsets.CChat.Entry_Type) = (int)type;
        *(uint*)(slot + SampOffsets.CChat.Entry_TextColor) = textColor;
        *(uint*)(slot + SampOffsets.CChat.Entry_PrefixColor) = prefixColor;
        *(int*)(slot + SampOffsets.CChat.Entry_Timestamp) = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        WriteAnsiToBuffer(slot + SampOffsets.CChat.Entry_Text, text, SampOffsets.CChat.Entry_TextSize);
        WriteAnsiToBuffer(slot + SampOffsets.CChat.Entry_Prefix, prefix, SampOffsets.CChat.Entry_PrefixSize);

        // Signal native Render to redraw
        *(int*)((byte*)instance + SampOffsets.CChat.Redraw) = 1;
    }

    // -- Internals --

    private static byte* GetEntryPtr(CChat* instance, int index) 
    {
        return (byte*)instance + SampOffsets.CChat.Entries + index * SampOffsets.CChat.EntrySize;
    }

    private static NativeChatEntry ParseEntry(byte* entry)
    {
        return new NativeChatEntry(
            Timestamp: *(int*)(entry + SampOffsets.CChat.Entry_Timestamp),
            Prefix: Marshal.PtrToStringAnsi((nint)(entry + SampOffsets.CChat.Entry_Prefix)),
            Text: Marshal.PtrToStringAnsi((nint)(entry + SampOffsets.CChat.Entry_Text)),
            Type: (EntryType)(*(int*)(entry + SampOffsets.CChat.Entry_Type)),
            TextColor: *(uint*)(entry + SampOffsets.CChat.Entry_TextColor),
            PrefixColor: *(uint*)(entry + SampOffsets.CChat.Entry_PrefixColor));
    }

    private static void WriteAnsiToBuffer(byte* dest, string? text, int maxBytes)
    {
        if (text is null)
        {
            *dest = 0;
            return;
        }

        using AnsiString ansi = AnsiString.Encode(text);
        byte* src = ansi;
        int len = 0;
        while (src[len] != 0 && len < maxBytes - 1)
        {
            len++;
        }

        Buffer.MemoryCopy(src, dest, maxBytes, len);
        dest[len] = 0;
    }

    private static CChat* RequireInstance()
    {
        CChat* instance = CurrentInstance;
        if (instance is null)
        {
            throw new InvalidOperationException("CChat instance is not available.");
        }

        return instance;
    }
}

public readonly record struct NativeChatEntry(
    int Timestamp,
    string? Prefix,
    string? Text,
    EntryType Type,
    uint TextColor,
    uint PrefixColor)
{
    public bool IsEmpty => Type == EntryType.None && Text is null;
}
