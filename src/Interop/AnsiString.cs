using System.Runtime.InteropServices;

namespace SFSharp;

internal readonly unsafe ref struct AnsiString
{
    public readonly byte* Pointer;
    public AnsiString(string? s)
    {
        Pointer = (byte*)Marshal.StringToHGlobalAnsi(s);
    }
    public void Dispose()
    {
        if (Pointer != null)
        {
            Marshal.FreeHGlobal((nint)Pointer);
        }
    }
    public static implicit operator byte*(AnsiString ansiString) => ansiString.Pointer;

    public static AnsiString Encode(string? s) => new(s);
    public static string? Decode(byte* pointer) => Marshal.PtrToStringAnsi((nint)pointer);
}