using System.Drawing;

namespace SFSharp;

public static class HookManager
{
    //public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static HookBase<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; } = new CChatAddEntryHook();
    public static HookBase<CDialogCloseArgs, NoRetValue> CDialogClose { get; } = !ModuleResolver.IsModuleLoaded("sampfuncs.asi") ? new CDialogCloseHook() : new CDialogCloseHook_SF();
    public static HookBase<CDialogHideArgs, NoRetValue> CDialogHide { get; } = new CDialogHideHook();
    public static HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow { get; } = new CDialogShowHook();
    public static HookBase<CInputSendArgs, bool> CInputSend { get; } = new CInputSendHook();
    public static HookBase<UpdateScoresPingsIpsArgs, NoRetValue> UpdateScoresPingsIps { get; } = new UpdateScoresPingsIpsHook();
}
