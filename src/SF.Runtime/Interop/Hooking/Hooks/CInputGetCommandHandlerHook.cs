using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct CInputCommandSendArgs(uint ThisPtr, string Text);

internal unsafe class CInputCommandSendHook : NativeHook<CInputCommandSendArgs, bool, CInputCommandSendHook.CInputSendNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CInputSendNative(IntPtr thisPtr, byte* text);

    private static string? _lastSentText;
    private static long _lastSentTick;

    public CInputCommandSendHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CInput.Send), new CInputSendNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr, byte* text)
    {
        bool dispatching = false;
        try
        {
            string decodedText = AnsiString.Decode(text) ?? string.Empty;
            long now = Environment.TickCount64;
            if (decodedText.StartsWith('/') && string.Equals(decodedText, _lastSentText, StringComparison.Ordinal) && now - _lastSentTick <= 250)
            {
                SFLog.Warn($"CInput::Send duplicate command suppressed text={decodedText}");
                return;
            }

            _lastSentText = decodedText;
            _lastSentTick = now;

            dispatching = true;
            bool handled = Process(new((uint)thisPtr, decodedText));
            if (handled)
            {
                SFLog.Debug($"CInput::Send command intercepted text={decodedText}");
            }
        }
        catch (Exception ex)
        {
            HookRuntime.ReportSafely(ex);
            if (!dispatching)
            {
                try { OriginalFunction(thisPtr, text); }
                catch (Exception originalError) { HookRuntime.ReportSafely(originalError); }
            }
        }
    }

    protected override bool InvokeOriginalFunction(CInputCommandSendArgs args)
    {
        using AnsiString text = AnsiString.Encode(args.Text);
        OriginalFunction((nint)args.ThisPtr, text);
        return false;
    }

}