using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

/// <summary>
/// Caller-owned native strings passed to <c>CDialog::Show</c>, kept alongside the decoded values so
/// the original pointers can be forwarded untouched when no sub-hook replaced them.
/// </summary>
public readonly record struct CDialogShowNativeStrings(
    nint Caption,
    nint Text,
    nint LeftButton,
    nint RightButton,
    string? DecodedCaption,
    string? DecodedText,
    string? DecodedLeftButton,
    string? DecodedRightButton);

public record struct CDialogShowHookArgs(
    uint ThisPtr,
    int Id,
    DialogStyle Style,
    string? Caption,
    string? Text,
    string? LeftButton,
    string? RightButton,
    bool ServerSide)
{
    public CDialogShowNativeStrings Native { get; init; }
}

internal unsafe class CDialogShowHook : NativeHook<CDialogShowHookArgs, NoRetValue, CDialogShowHook.CDialogShowNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate void CDialogShowNative(IntPtr thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide);

    /// <summary>
    /// Replacement buffers remain owned by this resident hook until the next dialog;
    /// unmodified strings keep their original caller-owned pointers.
    /// </summary>
    private static readonly nint[] _replacedStrings = new nint[4];

    public CDialogShowHook()
    {
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Show), new CDialogShowNative(HookProc));
    }

    private void HookProc(IntPtr thisPtr, int id, int type, byte* caption, byte* text, byte* leftButton, byte* rightButton, int serverSide)
    {
        bool dispatching = false;
        try
        {
            string? decodedCaption = AnsiString.Decode(caption);
            string? decodedText = AnsiString.Decode(text);
            string? decodedLeftButton = AnsiString.Decode(leftButton);
            string? decodedRightButton = AnsiString.Decode(rightButton);

            dispatching = true;
            Process(new(
                (uint)thisPtr,
                id,
                (DialogStyle)type,
                decodedCaption,
                decodedText,
                decodedLeftButton,
                decodedRightButton,
                serverSide != 0)
            {
                Native = new(
                    (nint)caption,
                    (nint)text,
                    (nint)leftButton,
                    (nint)rightButton,
                    decodedCaption,
                    decodedText,
                    decodedLeftButton,
                    decodedRightButton),
            });
        }
        catch (Exception ex)
        {
            HookRuntime.ReportSafely(ex);
            if (!dispatching)
            {
                try { OriginalFunction(thisPtr, id, type, caption, text, leftButton, rightButton, serverSide); }
                catch (Exception originalError) { HookRuntime.ReportSafely(originalError); }
            }
        }
    }

    protected override NoRetValue InvokeOriginalFunction(CDialogShowHookArgs args)
    {
        ReleaseReplacedStrings();

        CDialogShowNativeStrings native = args.Native;
        byte* caption = Resolve(0, args.Caption, native.DecodedCaption, native.Caption);
        byte* text = Resolve(1, args.Text, native.DecodedText, native.Text);
        byte* leftButton = Resolve(2, args.LeftButton, native.DecodedLeftButton, native.LeftButton);
        byte* rightButton = Resolve(3, args.RightButton, native.DecodedRightButton, native.RightButton);

        OriginalFunction((IntPtr)args.ThisPtr, args.Id, (int)args.Style, caption, text, leftButton, rightButton, args.ServerSide ? 1 : 0);
        return default;
    }

    private static byte* Resolve(int slot, string? current, string? decoded, nint originalPointer)
    {
        if (ReferenceEquals(current, decoded))
        {
            return (byte*)originalPointer;
        }

        nint replacement = Marshal.StringToHGlobalAnsi(current);
        _replacedStrings[slot] = replacement;
        return (byte*)replacement;
    }

    private static void ReleaseReplacedStrings()
    {
        for (int i = 0; i < _replacedStrings.Length; i++)
        {
            if (_replacedStrings[i] == 0)
            {
                continue;
            }

            Marshal.FreeHGlobal(_replacedStrings[i]);
            _replacedStrings[i] = 0;
        }
    }

}