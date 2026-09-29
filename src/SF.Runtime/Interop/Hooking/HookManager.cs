namespace SFSharp.Runtime.Interop.Hooking;

public static class HookManager
{
    private static HookBase<CDialogCloseArgs, NoRetValue>? _cDialogClose;
    private static HookBase<CDialogHideArgs, NoRetValue>? _cDialogHide;
    private static HookBase<CDialogShowHookArgs, NoRetValue>? _cDialogShow;
    private static HookBase<CInputCommandSendArgs, bool>? _cInputCommandSend;
    private static HookBase<UpdateScoresPingsIpsArgs, NoRetValue>? _updateScoresPingsIps;

    private static OutgoingRpcPacketHook? _outgoingRpcPacket;
    private static OutgoingPacketHook? _outgoingPacket;
    private static IncomingPacketHook? _incomingPacket;
    private static IncomingAZVoicePacketHook? _incomingAZVoicePacket;
    private static IncomingAZVoiceRpcHook? _incomingAZVoiceRpc;
    private static OutgoingAZVoiceRpcHook? _outgoingAZVoiceRpc;
    private static bool _azVoiceHookChecked;
    private static bool _azVoiceRpcHookChecked;
    private static bool _azVoiceOutRpcHookChecked;

    //public static Hook<PeekMessageArgs, PeekMessageResult> PeekMessage { get; } = new PeekMessageHook();
    public static HookBase<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; } = new CChatAddEntryHook();
    internal static IncomingRpcPacketHook IncomingRpcPacket { get; } = new IncomingRpcPacketHook();
    internal static OutgoingRpcPacketHook OutgoingRpcPacket => _outgoingRpcPacket ??= new OutgoingRpcPacketHook();
    internal static OutgoingPacketHook OutgoingPacket => _outgoingPacket ??= new OutgoingPacketHook();
    internal static IncomingPacketHook IncomingPacket => _incomingPacket ??= new IncomingPacketHook();

    internal static IncomingAZVoicePacketHook? IncomingAZVoicePacket
    {
        get
        {
            if (!_azVoiceHookChecked)
            {
                _azVoiceHookChecked = true;
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi") && IncomingAZVoicePacketHook.IsAvailable)
                    _incomingAZVoicePacket = new IncomingAZVoicePacketHook();
            }
            return _incomingAZVoicePacket;
        }
    }

    internal static IncomingAZVoiceRpcHook? IncomingAZVoiceRpc
    {
        get
        {
            if (!_azVoiceRpcHookChecked)
            {
                _azVoiceRpcHookChecked = true;
                // Resolved by a stable byte pattern now (see IncomingAZVoiceRpcHook); the
                // earlier crashes came from a stale module offset landing mid-function.
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi") && IncomingAZVoiceRpcHook.IsAvailable)
                    _incomingAZVoiceRpc = new IncomingAZVoiceRpcHook();
            }

            return _incomingAZVoiceRpc;
        }
    }

    internal static OutgoingAZVoiceRpcHook? OutgoingAZVoiceRpc
    {
        get
        {
            if (!_azVoiceOutRpcHookChecked)
            {
                _azVoiceOutRpcHookChecked = true;
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi") && OutgoingAZVoiceRpcHook.IsAvailable)
                    _outgoingAZVoiceRpc = new OutgoingAZVoiceRpcHook();
            }

            return _outgoingAZVoiceRpc;
        }
    }

    // Close and Hide are not installed. The Arizona client detours the CDialog::Close entry itself
    // and patches out samp's native dialog drawing, so a detour here can drop its CEF chain and
    // leave dialogs invisible; closing is observed through outgoing RPC 62 instead. Touching either
    // property re-installs the hook.
    public static HookBase<CDialogCloseArgs, NoRetValue> CDialogClose => _cDialogClose ??= !ModuleResolver.IsModuleLoaded("sampfuncs.asi") ? new CDialogCloseHook() : new CDialogCloseHook_SF();
    public static HookBase<CDialogHideArgs, NoRetValue> CDialogHide => _cDialogHide ??= new CDialogHideHook();
    public static HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow => _cDialogShow ??= new CDialogShowHook();
    public static HookBase<CInputCommandSendArgs, bool> CInputCommandSend => _cInputCommandSend ??= new CInputCommandSendHook();
    public static HookBase<UpdateScoresPingsIpsArgs, NoRetValue> UpdateScoresPingsIps => _updateScoresPingsIps ??= new UpdateScoresPingsIpsHook();
}
