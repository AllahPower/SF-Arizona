using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe class CChatAddEntryHook : Hook<CChatAddEntryArgs>, IDisposable
{
    private const int StolenBytesCount = 5;

    private static CChatAddEntryHook? _instance;
    private readonly delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void> _trampolinePtr;
    private readonly uint _functionAddress;

    public CChatAddEntryHook() : base(BaseFunction)
    {
        if (_instance is not null) throw new InvalidOperationException();

        _functionAddress = HookHelper.GetFunctionPtr("samp.dll", 0x67BE0);
        _trampolinePtr = (delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>)HookHelper.InstallSimpleHook(
            _functionAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>)&HookedFunction
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void HookedFunction(void* thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor)
    {
        if (_instance is null) throw new UnreachableException();

        var text = AnsiString.Decode(szText);
        var prefix = AnsiString.Decode(szPrefix);

        _instance.Process(new((uint)thisPtr, nType, text, prefix, textColor, prefixColor));
    }

    private static unsafe void BaseFunction(CChatAddEntryArgs args)
    {
        if (_instance is null) throw new UnreachableException();

        using var szText = AnsiString.Encode(args.Text);
        using var szPrefix = AnsiString.Encode(args.Prefix);

        _instance._trampolinePtr((void*)args.ThisPtr, args.Type, szText, szPrefix, args.TextColor, args.PrefixColor);
    }

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveSimpleHook(_functionAddress, StolenBytesCount, (uint)(delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>)&HookedFunction);
        _instance = null;
    }
}

public record CChatAddEntryArgs(uint ThisPtr, int Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

