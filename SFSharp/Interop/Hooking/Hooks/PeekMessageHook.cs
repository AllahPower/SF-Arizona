using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe class PeekMessageHook : HookBase<PeekMessageArgs, PeekMessageResult>
{
    private const int StolenBytesCount = 6;

    private static PeekMessageHook? _instance;
    private readonly uint _bufferAddress;
    private readonly uint _functionAddress;

    public PeekMessageHook()
    {
        if (_instance is not null) throw new InvalidOperationException();

        _functionAddress = HookHelper.GetFunctionPtr("gta_sa.exe", 0x748A57 - 0x400000);
        _bufferAddress = HookHelper.InstallCallHook(
            _functionAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Stdcall]<PeekMessageArgs, PeekMessageResult>)&HookedFunction
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe PeekMessageResult HookedFunction(PeekMessageArgs args)
    {
        if (_instance is null) throw new UnreachableException();

        return _instance.Process(args);
    }

    protected override PeekMessageResult InvokeOriginalFunction(PeekMessageArgs args)
    {
        return PeekMessageA(args);
    }

    [DllImport("user32.dll")] private static extern PeekMessageResult PeekMessageA(PeekMessageArgs args);

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveCallHook(_functionAddress, StolenBytesCount, _bufferAddress);
        _instance = null;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 4 * 5, Pack = 1)]
public struct PeekMessageArgs;

[StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
public struct PeekMessageResult;
