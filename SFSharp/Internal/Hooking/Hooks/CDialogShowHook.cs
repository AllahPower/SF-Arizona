using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public unsafe class CDialogShowHook : Hook<CDialogShowHookArgs, NoRetValue>, IDisposable
{
    private const int StolenBytesCount = 5;

    private static CDialogShowHook? _instance;
    private readonly delegate* unmanaged[Thiscall]<void*, int, int, byte*, byte*, byte*, byte*, int, void> _trampolinePtr;
    private readonly uint _functionAddress;

    public CDialogShowHook() : base(BaseFunction)
    {
        if (_instance is not null) throw new InvalidOperationException();

        _functionAddress = HookHelper.GetFunctionPtr("samp.dll", 0x6FFB0);
        _trampolinePtr = (delegate* unmanaged[Thiscall]<void*, int, int, byte*, byte*, byte*, byte*, int, void>)HookHelper.InstallJumpHook(
            _functionAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Thiscall]<void*, int, int, byte*, byte*, byte*, byte*, int, void>)&HookedFunction
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvThiscall)])]
    private static unsafe void HookedFunction(void* thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide)
    {
        if (_instance is null) throw new UnreachableException();

        try
        {
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
        catch (Exception ex)
        {
            SFCore.LogException(ex);
        }
    }

    private static unsafe NoRetValue BaseFunction(CDialogShowHookArgs args)
    {
        if (_instance is null) throw new UnreachableException();

        using var caption = AnsiString.Encode(args.Caption);
        using var text = AnsiString.Encode(args.Text);
        using var leftButton = AnsiString.Encode(args.LeftButton);
        using var rightButton = AnsiString.Encode(args.RightButton);

        _instance._trampolinePtr((void*)args.ThisPtr, args.Id, (int)args.Style, caption, text, leftButton, rightButton, args.ServerSide ? 1 : 0);
        return default;
    }

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveJumpHook(_functionAddress, StolenBytesCount, (uint)(delegate* unmanaged[Thiscall]<void*, int, int, byte*, byte*, byte*, byte*, int, void>)&HookedFunction);
        _instance = null;
    }
}

public record class CDialogShowHookArgs(uint ThisPtr, int Id, DialogStyle Style, string? Caption, string? Text, string? LeftButton, string? RightButton, bool ServerSide);