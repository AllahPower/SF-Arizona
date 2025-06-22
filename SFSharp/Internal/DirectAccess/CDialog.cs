using SFSharp;
using System.Runtime.InteropServices;

using unsafe ShowDelegate = delegate* unmanaged[Thiscall]<CDialog*, int, int, byte*, byte*, byte*, byte*, int, void>;
using unsafe GetSelectedIndexDelegate = delegate* unmanaged[Thiscall]<CDXUTListBox*, int, int>;

[StructLayout(LayoutKind.Explicit, Size = 689, Pack = 1)]
public unsafe struct CDialog
{
    private static readonly CDialog* _instance = *(CDialog**)HookHelper.GetFunctionPtr("samp.dll", 0x26EB50);
    public static ref CDialog Instance => ref *_instance;

    [FieldOffset(32)]
    public CDXUTListBox* ListBox;

    [FieldOffset(40)]
    public int IsActive;

    [FieldOffset(44)]
    public DialogStyle Style;

    [FieldOffset(48)]
    public uint Id;

    [FieldOffset(52)]
    public byte* Text;

    private static readonly ShowDelegate _show = (ShowDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x6FFB0);
    public void Show(int dialogId, DialogStyle style, string caption, string text, string leftButton, string rightButton, bool serverSide)
    {
        using var captionAnsi = AnsiString.Encode(caption);
        using var textAnsi = AnsiString.Encode(text);
        using var leftButtonAnsi = AnsiString.Encode(leftButton);
        using var rightButtonAnsi = AnsiString.Encode(rightButton);
        _show(_instance, dialogId, (int)style, captionAnsi, textAnsi, leftButtonAnsi, rightButtonAnsi, serverSide ? 1 : 0);
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

public unsafe struct CDXUTListBox
{
    private static readonly GetSelectedIndexDelegate _getSelectedIndex = (GetSelectedIndexDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x88E70);
    public int GetSelectedIndex(int listBoxId)
    {
        fixed (CDXUTListBox* thisPtr = &this)
        {
            return _getSelectedIndex(thisPtr, listBoxId);
        }
    }
}