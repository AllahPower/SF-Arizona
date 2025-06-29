using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
public unsafe delegate void CDialogShow(void* thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide);

public unsafe class CDialogShowHook : JumpHook<CDialogShowHookArgs, NoRetValue, CDialogShow>
{
    public CDialogShowHook() : base(
        stolenByteCount: 5,
        targetFunctionModule: "samp.dll",
        targetFunctionOffset: 0x6FFB0)
    { }

    protected override CDialogShow HookedFunction => HookProc;
    private unsafe void HookProc(void* thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide)
    {
        Process(new(
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

        Trampoline((void*)args.ThisPtr, args.Id, (int)args.Style, caption, text, leftButton, rightButton, args.ServerSide ? 1 : 0);
        return default;
    }
}

public record struct CDialogShowHookArgs(uint ThisPtr, int Id, DialogStyle Style, string? Caption, string? Text, string? LeftButton, string? RightButton, bool ServerSide);