namespace SFSharp;

public static class SF
{
    public static SFDialog Dialog { get; } = new SFDialog();
    public static SFKeyboard Keyboard { get; } = new SFKeyboard();
    public static SFChat Chat { get; } = new SFChat();
    public static SFPlayers Players { get; } = new SFPlayers();
    public static SFRpc Rpc { get; } = new SFRpc();
    public static SFPackets Packets { get; } = new SFPackets();

    public static string UserFilesDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files");
}
