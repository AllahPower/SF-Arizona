using System.Drawing;

namespace SFSharp;

public static class HookManager
{
    //public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static Hook<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; } = new CChatAddEntryHook();
    public static Hook<CDialogCloseArgs, NoRetValue> CDialogClose { get; } = HookHelper.GetFunctionPtr("sampfuncs.asi", 0) == 0 ? new CDialogCloseHook() : new CDialogCloseHook_SF();
    public static Hook<CDialogHideArgs, NoRetValue> CDialogHide { get; } = new CDialogHideHook();
    public static Hook<CDialogShowHookArgs, NoRetValue> CDialogShow { get; } = new CDialogShowHook();
}