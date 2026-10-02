namespace SFSharp.Runtime.Interop.Hooking;

/// <summary>
/// Owns the runtime's native hooks. The chat entry and incoming RPC hooks are installed on construction,
/// before CNetGame exists; every other hook is installed on first access.
/// </summary>
internal sealed class HookRegistry
{
    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    private HookBase<CDialogCloseArgs, NoRetValue>? _cDialogClose;
    private HookBase<CDialogHideArgs, NoRetValue>? _cDialogHide;
    private HookBase<CDialogShowHookArgs, NoRetValue>? _cDialogShow;
    private HookBase<CInputCommandSendArgs, bool>? _cInputCommandSend;
    private HookBase<UpdateScoresPingsIpsArgs, NoRetValue>? _updateScoresPingsIps;

    private OutgoingRpcPacketHook? _outgoingRpcPacket;
    private OutgoingPacketHook? _outgoingPacket;
    private IncomingPacketHook? _incomingPacket;
    private IncomingAZVoicePacketHook? _incomingAZVoicePacket;
    private IncomingAZVoiceRpcHook? _incomingAZVoiceRpc;
    private OutgoingAZVoiceRpcHook? _outgoingAZVoiceRpc;
    private bool _azVoiceHookChecked;
    private bool _azVoiceRpcHookChecked;
    private bool _azVoiceOutRpcHookChecked;

    public HookRegistry(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        CChatAddEntry = new CChatAddEntryHook();
        IncomingRpcPacket = new IncomingRpcPacketHook(filters, dispatcher);
    }

    public HookBase<CChatAddEntryArgs, NoRetValue> CChatAddEntry { get; }
    public IncomingRpcPacketHook IncomingRpcPacket { get; }
    public OutgoingRpcPacketHook OutgoingRpcPacket => _outgoingRpcPacket ??= new OutgoingRpcPacketHook(_filters, _dispatcher);
    public OutgoingPacketHook OutgoingPacket => _outgoingPacket ??= new OutgoingPacketHook(_filters, _dispatcher);
    public IncomingPacketHook IncomingPacket => _incomingPacket ??= new IncomingPacketHook(_filters, _dispatcher);

    public IncomingAZVoicePacketHook? IncomingAZVoicePacket
    {
        get
        {
            if (!_azVoiceHookChecked)
            {
                _azVoiceHookChecked = true;
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi") && IncomingAZVoicePacketHook.IsAvailable)
                    _incomingAZVoicePacket = new IncomingAZVoicePacketHook(_dispatcher);
            }
            return _incomingAZVoicePacket;
        }
    }

    public IncomingAZVoiceRpcHook? IncomingAZVoiceRpc
    {
        get
        {
            if (!_azVoiceRpcHookChecked)
            {
                _azVoiceRpcHookChecked = true;
                // Resolved by a stable byte pattern now (see IncomingAZVoiceRpcHook); the
                // earlier crashes came from a stale module offset landing mid-function.
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi") && IncomingAZVoiceRpcHook.IsAvailable)
                    _incomingAZVoiceRpc = new IncomingAZVoiceRpcHook(_dispatcher);
            }

            return _incomingAZVoiceRpc;
        }
    }

    public OutgoingAZVoiceRpcHook? OutgoingAZVoiceRpc
    {
        get
        {
            if (!_azVoiceOutRpcHookChecked)
            {
                _azVoiceOutRpcHookChecked = true;
                if (ModuleResolver.IsModuleLoaded("AZVoice.asi") && OutgoingAZVoiceRpcHook.IsAvailable)
                    _outgoingAZVoiceRpc = new OutgoingAZVoiceRpcHook(_dispatcher);
            }

            return _outgoingAZVoiceRpc;
        }
    }

    // Close and Hide are not installed. The Arizona client detours the CDialog::Close entry itself
    // and patches out samp's native dialog drawing, so a detour here can drop its CEF chain and
    // leave dialogs invisible; closing is observed through outgoing RPC 62 instead. Touching either
    // property re-installs the hook.
    public HookBase<CDialogCloseArgs, NoRetValue> CDialogClose => _cDialogClose ??= !ModuleResolver.IsModuleLoaded("sampfuncs.asi") ? new CDialogCloseHook() : new CDialogCloseSampfuncsHook();
    public HookBase<CDialogHideArgs, NoRetValue> CDialogHide => _cDialogHide ??= new CDialogHideHook();
    public HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow => _cDialogShow ??= new CDialogShowHook();
    public HookBase<CInputCommandSendArgs, bool> CInputCommandSend => _cInputCommandSend ??= new CInputCommandSendHook();
    public HookBase<UpdateScoresPingsIpsArgs, NoRetValue> UpdateScoresPingsIps => _updateScoresPingsIps ??= new UpdateScoresPingsIpsHook();
}
