namespace SFSharp;

public static class HookManager
{
    private static HookBase<CDialogCloseArgs, NoRetValue>? _cDialogClose;
    private static HookBase<CDialogHideArgs, NoRetValue>? _cDialogHide;
    private static HookBase<CDialogShowHookArgs, NoRetValue>? _cDialogShow;
    private static HookBase<CInputCommandSendArgs, bool>? _cInputCommandSend;
    private static HookBase<UpdateScoresPingsIpsArgs, NoRetValue>? _updateScoresPingsIps;

    //public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static HookBase<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; } = new CChatAddEntryHook();
    internal static IncomingRpcPacketHook IncomingRpcPacket { get; } = new IncomingRpcPacketHook();
    internal static OutgoingRpcPacketHook OutgoingRpcPacket { get; } = new OutgoingRpcPacketHook();
    public static HookBase<CDialogCloseArgs, NoRetValue> CDialogClose => _cDialogClose ??= !ModuleResolver.IsModuleLoaded("sampfuncs.asi") ? new CDialogCloseHook() : new CDialogCloseHook_SF();
    public static HookBase<CDialogHideArgs, NoRetValue> CDialogHide => _cDialogHide ??= new CDialogHideHook();
    public static HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow => _cDialogShow ??= new CDialogShowHook();
    public static HookBase<CInputCommandSendArgs, bool> CInputCommandSend => _cInputCommandSend ??= new CInputCommandSendHook();
    public static HookBase<UpdateScoresPingsIpsArgs, NoRetValue> UpdateScoresPingsIps => _updateScoresPingsIps ??= new UpdateScoresPingsIpsHook();
}