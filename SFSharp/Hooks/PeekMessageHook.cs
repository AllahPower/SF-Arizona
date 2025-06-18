using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe class PeekMessageHook : Hook<PeekMessageArgs, int>
{
    private const int StolenBytesCount = 6;

    private static PeekMessageHook? _instance;
    private readonly uint _bufferAddress;
    private readonly uint _functionAddress;

    public PeekMessageHook() : base(PeekMessageA)
    {
        if (_instance is not null) throw new InvalidOperationException();

        _functionAddress = HookHelper.GetFunctionPtr("gta_sa.exe", 0x748A57 - 0x400000);
        _bufferAddress = HookHelper.InstallCallHook(
            _functionAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Stdcall]<PeekMessageArgs, int>)&HookedFunction
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int HookedFunction(PeekMessageArgs args)
    {
        if (_instance is null) throw new UnreachableException();

       return _instance.Process(args);
    }

    [DllImport("user32.dll")] private static extern int PeekMessageA(PeekMessageArgs args);

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveCallHook(_functionAddress, StolenBytesCount, _bufferAddress);
        _instance = null;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 4 * 5, Pack = 1)]
public struct PeekMessageArgs;