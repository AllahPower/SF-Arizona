using SFSharp;
using System.Runtime.InteropServices;

using unsafe AddEntryDelegate = delegate* unmanaged[Thiscall]<CChat*, int, byte*, byte*, uint, uint, void>;

[StructLayout(LayoutKind.Explicit, Size = 25622, Pack = 1)]
public unsafe struct CChat
{
    private static readonly CChat* _instance = *(CChat**)HookHelper.GetFunctionPtr("samp.dll", 0x26EB80);
    public static ref CChat Instance => ref *_instance;

    private static readonly AddEntryDelegate _addEntry = (AddEntryDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x67BE0);
    public void AddEntry(EntryType type, string text, string prefix, uint textColor, uint prefixColor)
    {
        using var textAnsi = AnsiString.Encode(text);
        using var prefixAnsi = AnsiString.Encode(prefix);
        _addEntry(_instance, (int)type, textAnsi, prefixAnsi, textColor, prefixColor);
    }
}

public enum EntryType : int
{
    None = 0,
    Chat = 2,
    Info = 4,
    Debug = 8
}
