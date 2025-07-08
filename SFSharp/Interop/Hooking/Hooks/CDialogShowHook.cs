using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

using unsafe CDialogShow = delegate* unmanaged[Thiscall]<void*, int, int, byte*, byte*, byte*, byte*, int, void>;
public record struct CDialogShowHookArgs(uint ThisPtr, int Id, DialogStyle Style, string? Caption, string? Text, string? LeftButton, string? RightButton, bool ServerSide);

public unsafe class CDialogShowHook : JumpHook<CDialogShowHookArgs, NoRetValue>
{
    private static CDialogShowHook? _instance;
    public CDialogShowHook() : base(
        stolenByteCount: 5,
        functionAddress: HookHelper.GetFunctionPtr("samp.dll", 0x6FFB0)
    ) => _instance = this;

    protected override void* InjectedFunction => (CDialogShow)(&HookProc);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void HookProc(void* thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide)
    {
        if (_instance is null) throw new UnreachableException();

        _instance.Process(new(
            (uint)thisPtr,
            id,
            (DialogStyle)type,
            AnsiString.Decode(caption),
            AnsiString.Decode(text),
            AnsiString.Decode(leftButton),
            AnsiString.Decode(rightButton),
            serverSide != 0
        ));
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogShowHookArgs args)
    {
        using var caption = AnsiString.Encode(args.Caption);
        using var text = AnsiString.Encode(args.Text);
        using var leftButton = AnsiString.Encode(args.LeftButton);
        using var rightButton = AnsiString.Encode(args.RightButton);

        ((CDialogShow)OriginalFunction)((void*)args.ThisPtr, args.Id, (int)args.Style, caption, text, leftButton, rightButton, args.ServerSide ? 1 : 0);
        return default;
    }
}

