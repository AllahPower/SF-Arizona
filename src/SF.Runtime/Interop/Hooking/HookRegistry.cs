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
    private OutgoingDataPacketHook? _outgoingDataPacket;
    private IncomingPacketHook? _incomingPacket;

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
    public OutgoingDataPacketHook OutgoingDataPacket => _outgoingDataPacket ??= new OutgoingDataPacketHook(_filters, _dispatcher);
    public IncomingPacketHook IncomingPacket => _incomingPacket ??= new IncomingPacketHook(_filters, _dispatcher);

    public HookBase<CDialogCloseArgs, NoRetValue> CDialogClose => _cDialogClose ??= new CDialogCloseHook();
    public HookBase<CDialogHideArgs, NoRetValue> CDialogHide => _cDialogHide ??= new CDialogHideHook();
    public HookBase<CDialogShowHookArgs, NoRetValue> CDialogShow => _cDialogShow ??= new CDialogShowHook();
    public HookBase<CInputCommandSendArgs, bool> CInputCommandSend => _cInputCommandSend ??= new CInputCommandSendHook();
    public HookBase<UpdateScoresPingsIpsArgs, NoRetValue> UpdateScoresPingsIps => _updateScoresPingsIps ??= new UpdateScoresPingsIpsHook();
}
