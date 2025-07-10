using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe CChatAddEntry = delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>;
public record struct CChatAddEntryArgs(uint ThisPtr, int Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

public unsafe class CChatAddEntryHook : JumpHook<CChatAddEntryArgs, NoRetValue>
{
    private static CChatAddEntryHook? _instance;
    public CChatAddEntryHook() : base(
        stolenByteCount: 5,
        functionAddress: HookHelper.GetFunctionPtr("samp.dll", 0x67BE0)
    ) => _instance = this;

    protected override void* InjectedFunction => (CChatAddEntry)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static void HookProc(void* thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor)
    {
        if (_instance is null) throw new UnreachableException();

        var text = AnsiString.Decode(szText);
        var prefix = AnsiString.Decode(szPrefix);

        _instance.Process(new((uint)thisPtr, nType, text, prefix, textColor, prefixColor));
    }

    protected override NoRetValue InvokeOriginalFunction(CChatAddEntryArgs args)
    {
        using var szText = AnsiString.Encode(args.Text);
        using var szPrefix = AnsiString.Encode(args.Prefix);

        ((CChatAddEntry)OriginalFunction)((void*)args.ThisPtr, args.Type, szText, szPrefix, args.TextColor, args.PrefixColor);
        return default;
    }
    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}


