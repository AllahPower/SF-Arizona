using System;
using System.Runtime.InteropServices;

using unsafe ShowDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CDialog*, int, int, byte*, byte*, byte*, byte*, int, void>;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Explicit, Size = 689, Pack = 1)]
public unsafe ref struct CDialog
{
    private static readonly nuint _instanceAddress = (nuint)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Instance);
    private static CDialog* CurrentInstance => *(CDialog**)_instanceAddress;
    public static ref readonly CDialog Instance => ref *RequireInstance();

    [FieldOffset(32)]
    public CDXUTListBox* ListBox;

    [FieldOffset(40)]
    private int isActive;
    public bool IsActive => isActive != 0;

    [FieldOffset(44)]
    public DialogStyle Style;

    [FieldOffset(48)]
    public uint Id;

    [FieldOffset(52)]
    public byte* Text;

    private static readonly ShowDelegate _show = (ShowDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CDialog.Show);
    public void Show(int dialogId, DialogStyle style, string caption, string text, string leftButton, string rightButton, bool serverSide)
    {
        var instance = RequireInstance();
        using var captionAnsi = AnsiString.Encode(caption);
        using var textAnsi = AnsiString.Encode(text);
        using var leftButtonAnsi = AnsiString.Encode(leftButton);
        using var rightButtonAnsi = AnsiString.Encode(rightButton);
        _show(instance, dialogId, (int)style, captionAnsi, textAnsi, leftButtonAnsi, rightButtonAnsi, serverSide ? 1 : 0);
    }

    private static CDialog* RequireInstance()
    {
        var instance = CurrentInstance;
        if (instance is null)
        {
            throw new InvalidOperationException("CDialog instance is not available.");
        }

        return instance;
    }
}

public enum DialogStyle
{
    MsgBox = 0,
    Input = 1,
    List = 2,
    Password = 3,
    TabList = 4,
    TabListHeaders = 5,
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct CDXUTListBox
{
    [FieldOffset(323)]
    public int SelectedIndex;
}
