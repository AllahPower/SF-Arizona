using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe class CChatAddEntryHook : IDisposable {
    private const int StolenBytesCount = 5;

    private static CChatAddEntryHook? _instance;
    private readonly delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void> trampolinePtr;
    private readonly uint functionAddress;

    public CChatAddEntryHook() {
        if (_instance is not null) throw new InvalidOperationException();

        functionAddress = HookHelper.GetFunctionPtr("samp.dll", 0x67BE0);
        HookHelper.InstallSimpleHook(functionAddress, StolenBytesCount, (uint)(delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>)&MyAddEntry);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void MyAddEntry(void* thisPtr, int nType, byte* szText, byte* szPrefix, uint textColor, uint prefixColor) {
        if (_instance is null) throw new UnreachableException();

        var text = AnsiString.Decode(szText);
        var prefix = AnsiString.Decode(szPrefix);

        _instance.trampolinePtr(thisPtr, nType, szText, szPrefix, textColor, prefixColor);
    }

    public void Dispose() {
        HookHelper.RemoveSimpleHook(functionAddress, StolenBytesCount, (uint)(delegate* unmanaged[Thiscall]<void*, int, byte*, byte*, uint, uint, void>)&MyAddEntry);
        _instance = null;
    }
}

public record CChatAddEntryArgs(int Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

