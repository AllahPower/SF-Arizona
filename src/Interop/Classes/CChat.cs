using SFSharp;
using System;
using System.Runtime.InteropServices;

using unsafe AddChatMessageDelegate = delegate* unmanaged[Thiscall]<CChat*, byte*, uint, byte*, void>;
using unsafe AddEntryDelegate = delegate* unmanaged[Thiscall]<CChat*, int, byte*, byte*, uint, uint, void>;
using unsafe AddMessageDelegate = delegate* unmanaged[Thiscall]<CChat*, uint, byte*, void>;

[StructLayout(LayoutKind.Explicit, Size = 25622, Pack = 1)]
public unsafe ref struct CChat
{
    private static readonly nuint _instanceAddress = (nuint)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.Instance);
    private static CChat* CurrentInstance => *(CChat**)_instanceAddress;
    public static ref readonly CChat Instance => ref *RequireInstance();

    private static readonly AddEntryDelegate _addEntry = (AddEntryDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddEntry);
    private static readonly AddChatMessageDelegate _addChatMessage = (AddChatMessageDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddChatMessage);
    private static readonly AddMessageDelegate _addMessage = (AddMessageDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddMessage);

    public void AddEntry(EntryType type, string? text, string? prefix, uint textColor, uint prefixColor)
    {
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

public enum EntryType : int
{
    None = 0,
    Chat = 2,
    Info = 4,
    Debug = 8
}
