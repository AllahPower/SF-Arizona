using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

using unsafe CChatAddMessageDirect = delegate* unmanaged[Thiscall]<void*, uint, byte*, void>;

public record struct CChatAddMessageArgs(uint ThisPtr, uint Color, string? Text);

internal unsafe class CChatAddMessageHook : NativeHook<CChatAddMessageArgs, NoRetValue, CChatAddMessageHook.CChatAddMessageNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CChatAddMessageNative(IntPtr thisPtr, uint color, byte* szText);

    private static CChatAddMessageHook? _instance;

    public CChatAddMessageHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddMessage), new CChatAddMessageNative(HookProc));
    }

    private static unsafe void HookProc(IntPtr thisPtr, uint color, byte* szText)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new((uint)thisPtr, color, AnsiString.Decode(szText)));
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddMessageArgs args)
    {
        using AnsiString szText = AnsiString.Encode(args.Text);
        using IDisposable _ = SuppressHook();

        ((CChatAddMessageDirect)TargetAddress)((void*)args.ThisPtr, args.Color, szText);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
