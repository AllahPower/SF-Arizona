using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe CInputGetCommandHandler = delegate* unmanaged[Thiscall]<void*, byte*, delegate* unmanaged[Cdecl]<byte*, void>>;
public record struct CInputGetCommandHandlerArgs(uint ThisPtr, string CommandName);
public unsafe struct CInputGetCommandHandlerRetValue
{
    public delegate* unmanaged[Cdecl]<byte*, void> Handler;
    public static implicit operator CInputGetCommandHandlerRetValue(delegate* unmanaged[Cdecl]<byte*, void> handler) => new() { Handler = handler };
    public static implicit operator delegate* unmanaged[Cdecl]<byte*, void>(CInputGetCommandHandlerRetValue retValue) => retValue.Handler;
}

public unsafe class CInputGetCommandHandlerHook : JumpHook<CInputGetCommandHandlerArgs, CInputGetCommandHandlerRetValue>
{
    private static CInputGetCommandHandlerHook? _instance;
    public CInputGetCommandHandlerHook() : base(
        stolenByteCount: 6,
        targetFunctionModule: "samp.dll",
        targetFunctionOffset: 0x69710
    ) => _instance = this;

    protected override void* InjectedFunction => (CInputGetCommandHandler)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe delegate* unmanaged[Cdecl]<byte*, void> HookProc(void* thisPtr, byte* commandName)
    {
        if (_instance is null) throw new UnreachableException();

        // CInput::Process ensures that commandName has at least one character
        return _instance.Process(new((uint)thisPtr, AnsiString.Decode(commandName) ?? throw new UnreachableException()));
    }

    protected override unsafe CInputGetCommandHandlerRetValue InvokeOriginalFunction(CInputGetCommandHandlerArgs args)
    {
        using var commandName = AnsiString.Encode(args.CommandName);

        var handler = ((CInputGetCommandHandler)OriginalFunction)((void*)args.ThisPtr, commandName);
        return handler;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}

