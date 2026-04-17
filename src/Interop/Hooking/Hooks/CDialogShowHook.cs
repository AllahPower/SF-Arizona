using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

using unsafe CDialogShowDirect = delegate* unmanaged[Thiscall]<void*, int, int, byte*, byte*, byte*, byte*, int, void>;

public record struct CDialogShowHookArgs(uint ThisPtr, int Id, DialogStyle Style, string? Caption, string? Text, string? LeftButton, string? RightButton, bool ServerSide);

internal unsafe class CDialogShowHook : NativeHook<CDialogShowHookArgs, NoRetValue, CDialogShowHook.CDialogShowNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CDialogShowNative(IntPtr thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide);

    private static CDialogShowHook? _instance;

    public CDialogShowHook()
    {
        _instance = this;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Show), new CDialogShowNative(HookProc));
    }

    private static unsafe void HookProc(IntPtr thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

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

    protected override NoRetValue InvokeOriginalFunction(CDialogShowHookArgs args)
    {
        using var caption = AnsiString.Encode(args.Caption);
        using var text = AnsiString.Encode(args.Text);
        using var leftButton = AnsiString.Encode(args.LeftButton);
        using var rightButton = AnsiString.Encode(args.RightButton);
        using var _ = SuppressHook();

        ((CDialogShowDirect)TargetAddress)((void*)args.ThisPtr, args.Id, (int)args.Style, caption, text, leftButton, rightButton, args.ServerSide ? 1 : 0);
        return default;
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
