using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
public unsafe delegate void CChatAddEntry(void* thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor);

public unsafe class CChatAddEntryHook : JumpHook<CChatAddEntryArgs, NoRetValue, CChatAddEntry>
{
    public CChatAddEntryHook() : base(
        stolenByteCount: 5,
        targetFunctionModule: "samp.dll",
        targetFunctionOffset: 0x67BE0
    )
    { }

    protected override CChatAddEntry HookedFunction => HookProc;
    private void HookProc(void* thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor)
    {
        var text = AnsiString.Decode(szText);
        var prefix = AnsiString.Decode(szPrefix);

        Process(new((uint)thisPtr, nType, text, prefix, textColor, prefixColor));
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddEntryArgs args)
    {
        using var szText = AnsiString.Encode(args.Text);
        using var szPrefix = AnsiString.Encode(args.Prefix);

        Trampoline((void*)args.ThisPtr, args.Type, szText, szPrefix, args.TextColor, args.PrefixColor);
        return default;
    }
}

public record CChatAddEntryArgs(uint ThisPtr, int Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);
