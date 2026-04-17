using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

using unsafe CInputSendDirect = delegate* unmanaged[Thiscall]<void*, byte*, void>;

public record struct CInputCommandSendArgs(uint ThisPtr, string Text);

internal unsafe class CInputCommandSendHook : NativeHook<CInputCommandSendArgs, bool, CInputCommandSendHook.CInputSendNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CInputSendNative(IntPtr thisPtr, byte* text);

    private static CInputCommandSendHook? _instance;
    private static string? _lastSentText;
    private static long _lastSentTick;

    public CInputCommandSendHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CInput.Send), new CInputSendNative(HookProc));
    }

    private static unsafe void HookProc(IntPtr thisPtr, byte* text)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        string decodedText = AnsiString.Decode(text) ?? throw new UnreachableException();
        long now = Environment.TickCount64;
        if (decodedText.StartsWith('/') && string.Equals(decodedText, _lastSentText, StringComparison.Ordinal) && now - _lastSentTick <= 250)
        {
            SFLog.Warn($"CInput::Send duplicate command suppressed text={decodedText}");
            return;
        }

        _lastSentText = decodedText;
        _lastSentTick = now;

        bool handled = _instance.Process(new((uint)thisPtr, decodedText));
        if (handled)
        {
            SFLog.Debug($"CInput::Send command intercepted text={decodedText}");
        }
    }

    protected override bool InvokeOriginalFunction(CInputCommandSendArgs args)
    {
        using AnsiString text = AnsiString.Encode(args.Text);
        using IDisposable _ = SuppressHook();
        ((CInputSendDirect)TargetAddress)((void*)args.ThisPtr, text);
        return false;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
