namespace SFSharp;

public interface ISFComponent
{
    void Initialize();
}

public static class SF
{
    public static SFDialog Dialog { get; } = new SFDialog();
    public static SFKeyboard Keyboard { get; } = new SFKeyboard();
    public static SFChat Chat { get; } = new SFChat();
    public static SFPlayers Players { get; } = new SFPlayers();

    public static string UserFilesDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GTA San Andreas User Files");
}
