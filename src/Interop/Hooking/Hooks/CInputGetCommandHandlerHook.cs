using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe CInputSendDirect = delegate* unmanaged[Thiscall]<void*, byte*, void>;

public record struct CInputSendArgs(uint ThisPtr, string Text);

internal unsafe class CInputSendHook : NativeHook<CInputSendArgs, bool, CInputSendHook.CInputSendNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CInputSendNative(IntPtr thisPtr, byte* text);

    private static CInputSendHook? _instance;

    public CInputSendHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", 0x69190), new CInputSendNative(HookProc));
    }

    private static unsafe void HookProc(IntPtr thisPtr, byte* text)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        var decodedText = AnsiString.Decode(text) ?? throw new UnreachableException();
        var handled = _instance.Process(new((uint)thisPtr, decodedText));
        if (handled)
        {
            SFLog.Info($"CInput::Send intercepted text={decodedText}");
            return;
        }

        SFLog.Info($"CInput::Send pass-through text={decodedText}");
        using var _ = _instance.SuppressHook();
        ((CInputSendDirect)_instance.TargetAddress)((void*)thisPtr, text);
    }

    protected override bool InvokeOriginalFunction(CInputSendArgs args)
    {
        using var text = AnsiString.Encode(args.Text);
        using var _ = SuppressHook();
        ((CInputSendDirect)TargetAddress)((void*)args.ThisPtr, text);
        return false;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
