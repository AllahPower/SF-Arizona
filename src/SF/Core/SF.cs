namespace SFSharp;

public static class SF
{
    public static SFDialog Dialog { get; } = new SFDialog();
    public static SFKeyboard Keyboard { get; } = new SFKeyboard();
    public static SFChat Chat { get; } = new SFChat();
    public static SFPlayers Players { get; } = new SFPlayers();
    public static SFVehicles Vehicles { get; } = new SFVehicles();
    public static SFGamePools Pools { get; } = new SFGamePools();
    public static SFRpc Rpc { get; } = new SFRpc();
    public static SFPackets Packets { get; } = new SFPackets();
    public static SFArizonaPackets Arizona { get; } = new SFArizonaPackets();
    public static SFArizonaChat ArizonaChat { get; } = new SFArizonaChat();
    public static SFPacketParsers PacketParsers { get; } = new SFPacketParsers();
    public static SFRpcParsers RpcParsers { get; } = new SFRpcParsers();
    public static SFCamera Camera { get; } = new SFCamera();
    public static SFEvents Events { get; } = new SFEvents();

    public static string UserFilesDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files");
}
