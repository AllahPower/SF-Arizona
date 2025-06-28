using System.Drawing;

namespace SFSharp;

public static class HookManager
{
    //public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static HookBase<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; } = new CChatAddEntryHook();
    public static HookBase<CDialogCloseArgs, NoRetValue> CDialogClose { get; } = HookHelper.GetFunctionPtr("sampfuncs.asi", 0) == 0 ? new CDialogCloseHook() : new CDialogCloseHook_SF();
    public static HookBase<CDialogHideArgs, NoRetValue> CDialogHide { get; } = new CDialogHideHook();
    public static HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow { get; } = new CDialogShowHook();
}