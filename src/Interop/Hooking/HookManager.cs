namespace SFSharp;

public static class HookManager
{
    private static HookBase<CDialogCloseArgs, NoRetValue>? _cDialogClose;
    private static HookBase<CDialogHideArgs, NoRetValue>? _cDialogHide;
    private static HookBase<CDialogShowHookArgs, NoRetValue>? _cDialogShow;
    private static HookBase<CInputCommandSendArgs, bool>? _cInputCommandSend;
    private static HookBase<UpdateScoresPingsIpsArgs, NoRetValue>? _updateScoresPingsIps;

    internal static OutgoingRpcPacketHook OutgoingRpcPacket { get; } = new OutgoingRpcPacketHook();
    private static OutgoingPacketHook? _outgoingPacket;
    private static IncomingPacketHook? _incomingPacket;
    private static IncomingAZVoicePacketHook? _incomingAZVoicePacket;
    private static IncomingAZVoiceRpcHook? _incomingAZVoiceRpc;
    private static bool _azVoiceHookChecked;

    //public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static HookBase<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; } = new CChatAddEntryHook();
    internal static IncomingRpcPacketHook IncomingRpcPacket { get; } = new IncomingRpcPacketHook();
    internal static OutgoingPacketHook OutgoingPacket => _outgoingPacket ??= new OutgoingPacketHook();
    internal static IncomingPacketHook IncomingPacket => _incomingPacket ??= new IncomingPacketHook();

    internal static IncomingAZVoicePacketHook? IncomingAZVoicePacket
    {
        get
        {
            if (!_azVoiceHookChecked)
            {
                _azVoiceHookChecked = true;
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi"))
                    _incomingAZVoicePacket = new IncomingAZVoicePacketHook();
            }
            return _incomingAZVoicePacket;
        }
    }

    internal static IncomingAZVoiceRpcHook? IncomingAZVoiceRpc
    {
        get
        {
            if (!_azVoiceHookChecked)
            {
                _ = IncomingAZVoicePacket;
            }

            if (_incomingAZVoiceRpc is null && ModuleResolver.IsModuleLoaded("AZVoice.asi"))
            {
                _incomingAZVoiceRpc = new IncomingAZVoiceRpcHook();
            }

            return _incomingAZVoiceRpc;
        }
    }
    public static HookBase<CDialogCloseArgs, NoRetValue> CDialogClose => _cDialogClose ??= !ModuleResolver.IsModuleLoaded("sampfuncs.asi") ? new CDialogCloseHook() : new CDialogCloseHook_SF();
    public static HookBase<CDialogHideArgs, NoRetValue> CDialogHide => _cDialogHide ??= new CDialogHideHook();
    public static HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow => _cDialogShow ??= new CDialogShowHook();
    public static HookBase<CInputCommandSendArgs, bool> CInputCommandSend => _cInputCommandSend ??= new CInputCommandSendHook();
    public static HookBase<UpdateScoresPingsIpsArgs, NoRetValue> UpdateScoresPingsIps => _updateScoresPingsIps ??= new UpdateScoresPingsIpsHook();
}
