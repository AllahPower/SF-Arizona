using SFSharp;
using System;
using System.Runtime.InteropServices;

using unsafe AddEntryDelegate = delegate* unmanaged[Thiscall]<CChat*, int, byte*, byte*, uint, uint, void>;

[StructLayout(LayoutKind.Explicit, Size = 25622, Pack = 1)]
public unsafe ref struct CChat
{
    private static readonly nuint _instanceAddress = (nuint)ModuleResolver.GetProcAddress("samp.dll", 0x26E8C8);
    private static CChat* CurrentInstance => *(CChat**)_instanceAddress;
    public static ref readonly CChat Instance => ref *RequireInstance();

    private static readonly AddEntryDelegate _addEntry = (AddEntryDelegate)ModuleResolver.GetProcAddress("samp.dll", 0x67460);
    public void AddEntry(EntryType type, string? text, string? prefix, uint textColor, uint prefixColor)
    {
        var instance = RequireInstance();
        using var textAnsi = AnsiString.Encode(text);
        using var prefixAnsi = AnsiString.Encode(prefix);
        _addEntry(instance, (int)type, textAnsi, prefixAnsi, textColor, prefixColor);
    }

    private static CChat* RequireInstance()
    {
        var instance = CurrentInstance;
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
