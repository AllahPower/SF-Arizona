using SFSharp;
using System.Runtime.InteropServices;

using unsafe SendDelegate = delegate* unmanaged[Thiscall]<CInput*, byte*, void>;

[StructLayout(LayoutKind.Explicit, Size = 7500, Pack = 1)]
public unsafe struct CInput
{
    private static readonly CInput* _instance = *(CInput**)HookHelper.GetFunctionPtr("samp.dll", 0x26EB84);
    public static ref CInput Instance => ref *_instance;

    private static readonly SendDelegate _send = (SendDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x69900);
    public void Send(string text)
    {
        using var textAnsi = AnsiString.Encode(text);
        _send(_instance, textAnsi);
    }
}
