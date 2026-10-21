using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct CChatAddChatMessageArgs(uint ThisPtr, string? Prefix, uint PrefixColor, string? Text);

internal unsafe class CChatAddChatMessageHook : NativeHook<CChatAddChatMessageArgs, NoRetValue, CChatAddChatMessageHook.CChatAddChatMessageNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CChatAddChatMessageNative(IntPtr thisPtr, byte* szPrefix, uint prefixColor, byte* szText);

    public CChatAddChatMessageHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddChatMessage), new CChatAddChatMessageNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr, byte* szPrefix, uint prefixColor, byte* szText)
    {
        bool dispatching = false;
        try
        {
            string? prefix = AnsiString.Decode(szPrefix);
            string? text = AnsiString.Decode(szText);
            dispatching = true;
            Process(new((uint)thisPtr, prefix, prefixColor, text));
        }
        catch (Exception ex)
        {
            HookRuntime.ReportSafely(ex);
            if (!dispatching)
            {
                try { OriginalFunction(thisPtr, szPrefix, prefixColor, szText); }
                catch (Exception originalError) { HookRuntime.ReportSafely(originalError); }
            }
        }
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddChatMessageArgs args)
    {
        using AnsiString szPrefix = AnsiString.Encode(args.Prefix);
        using AnsiString szText = AnsiString.Encode(args.Text);
        OriginalFunction((nint)args.ThisPtr, szPrefix, args.PrefixColor, szText);
        return default;
    }

}
