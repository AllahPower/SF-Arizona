using SFSharp;
using System;
using System.Runtime.InteropServices;

using unsafe SendDelegate = delegate* unmanaged[Thiscall]<CInput*, byte*, void>;

[StructLayout(LayoutKind.Explicit, Size = 6908, Pack = 1)]
public unsafe ref struct CInput
{
    private static readonly nuint _instanceAddress = (nuint)ModuleResolver.GetProcAddress("samp.dll", 0x26E8CC);
    private static CInput* CurrentInstance => *(CInput**)_instanceAddress;
    public static ref readonly CInput Instance => ref *RequireInstance();

    private static readonly SendDelegate _send = (SendDelegate)ModuleResolver.GetProcAddress("samp.dll", 0x69190);
    public void Send(string text)
    {
        var instance = RequireInstance();
        using var textAnsi = AnsiString.Encode(text);
        _send(instance, textAnsi);
    }

    private static CInput* RequireInstance()
    {
        var instance = CurrentInstance;
        if (instance is null)
        {
            throw new InvalidOperationException("CInput instance is not available.");
        }

        return instance;
    }
}
