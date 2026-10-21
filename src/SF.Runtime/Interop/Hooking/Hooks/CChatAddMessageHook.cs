using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

public record struct CChatAddMessageArgs(uint ThisPtr, uint Color, string? Text);

internal unsafe class CChatAddMessageHook : NativeHook<CChatAddMessageArgs, NoRetValue, CChatAddMessageHook.CChatAddMessageNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CChatAddMessageNative(IntPtr thisPtr, uint color, byte* szText);

    public CChatAddMessageHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddMessage), new CChatAddMessageNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr, uint color, byte* szText)
    {
        bool dispatching = false;
        try
        {
            string? text = AnsiString.Decode(szText);
            dispatching = true;
            Process(new((uint)thisPtr, color, text));
        }
        catch (Exception ex)
        {
            HookRuntime.ReportSafely(ex);
            if (!dispatching)
            {
                try { OriginalFunction(thisPtr, color, szText); }
                catch (Exception originalError) { HookRuntime.ReportSafely(originalError); }
            }
        }
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddMessageArgs args)
    {
        using AnsiString szText = AnsiString.Encode(args.Text);
        OriginalFunction((nint)args.ThisPtr, args.Color, szText);
        return default;
    }

}
