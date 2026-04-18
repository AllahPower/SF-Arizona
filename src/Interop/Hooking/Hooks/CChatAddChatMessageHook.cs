using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking;

using unsafe CChatAddChatMessageDirect = delegate* unmanaged[Thiscall]<void*, byte*, uint, byte*, void>;

public record struct CChatAddChatMessageArgs(uint ThisPtr, string? Prefix, uint PrefixColor, string? Text);

internal unsafe class CChatAddChatMessageHook : NativeHook<CChatAddChatMessageArgs, NoRetValue, CChatAddChatMessageHook.CChatAddChatMessageNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CChatAddChatMessageNative(IntPtr thisPtr, byte* szPrefix, uint prefixColor, byte* szText);

    private static CChatAddChatMessageHook? _instance;

    public CChatAddChatMessageHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddChatMessage), new CChatAddChatMessageNative(HookProc));
    }

    private static unsafe void HookProc(IntPtr thisPtr, byte* szPrefix, uint prefixColor, byte* szText)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new((uint)thisPtr, AnsiString.Decode(szPrefix), prefixColor, AnsiString.Decode(szText)));
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddChatMessageArgs args)
    {
        using AnsiString szPrefix = AnsiString.Encode(args.Prefix);
        using AnsiString szText = AnsiString.Encode(args.Text);
        using IDisposable _ = SuppressHook();

        ((CChatAddChatMessageDirect)TargetAddress)((void*)args.ThisPtr, szPrefix, args.PrefixColor, szText);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
