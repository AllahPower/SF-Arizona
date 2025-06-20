using System.Drawing;

namespace SFSharp;

public static class HookManager
{
    public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static Hook<CChatAddEntryArgs> CChatAddEntry { get; } = new CChatAddEntryHook();
    public static Hook<CDialogCloseArgs> CDialogClose { get; } = HookHelper.GetFunctionPtr("sampfuncs.asi", 0) == 0 ? new CDialogCloseHook() : new CDialogCloseHook_SF();
}