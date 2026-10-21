using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct CChatAddEntryArgs(uint ThisPtr, int Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

internal unsafe class CChatAddEntryHook : NativeHook<CChatAddEntryArgs, NoRetValue, CChatAddEntryHook.CChatAddEntryNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CChatAddEntryNative(IntPtr thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor);

    public CChatAddEntryHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddEntry), new CChatAddEntryNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor)
    {
        bool dispatching = false;
        try
        {
            string? text = AnsiString.Decode(szText);
            string? prefix = AnsiString.Decode(szPrefix);
            dispatching = true;
            Process(new((uint)thisPtr, nType, text, prefix, textColor, prefixColor));
        }
        catch (Exception ex)
        {
            HookRuntime.ReportSafely(ex);
            if (!dispatching)
            {
                try { OriginalFunction(thisPtr, nType, szText, szPrefix, textColor, prefixColor); }
                catch (Exception originalError) { HookRuntime.ReportSafely(originalError); }
            }
        }
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddEntryArgs args)
    {
        using var szText = AnsiString.Encode(args.Text);
        using var szPrefix = AnsiString.Encode(args.Prefix);
        OriginalFunction((nint)args.ThisPtr, args.Type, szText, szPrefix, args.TextColor, args.PrefixColor);
        return default;
    }

}
