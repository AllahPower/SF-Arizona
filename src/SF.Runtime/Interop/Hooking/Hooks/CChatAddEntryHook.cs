using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking;

using unsafe CChatAddEntryDirect = delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>;

public record struct CChatAddEntryArgs(uint ThisPtr, int Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

internal unsafe class CChatAddEntryHook : NativeHook<CChatAddEntryArgs, NoRetValue, CChatAddEntryHook.CChatAddEntryNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CChatAddEntryNative(IntPtr thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor);

    private static CChatAddEntryHook? _instance;

    public CChatAddEntryHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CChat.AddEntry), new CChatAddEntryNative(HookProc));
    }

    private static unsafe void HookProc(IntPtr thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        _instance.Process(new((uint)thisPtr, nType, AnsiString.Decode(szText), AnsiString.Decode(szPrefix), textColor, prefixColor));
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddEntryArgs args)
    {
        using var szText = AnsiString.Encode(args.Text);
        using var szPrefix = AnsiString.Encode(args.Prefix);
        using var _ = SuppressHook();

        ((CChatAddEntryDirect)TargetAddress)((void*)args.ThisPtr, args.Type, szText, szPrefix, args.TextColor, args.PrefixColor);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
