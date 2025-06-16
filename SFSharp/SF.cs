namespace SFSharp;

public record PlayerInfo(string Name, int Id, int Score, int Ping);

public static partial class SF
{
    public static string SFSharpDirectory => Path.Combine(Environment.CurrentDirectory, "SFSharp");

    public static void BeginInvoke(SendOrPostCallback callback)
    {
        SFCore.PostToMainLoop(callback);
    }

    public static bool IsPlayerDefined(int playerId) => SFCore.IsPlayerDefined(playerId);
    public static string? GetPlayerName(int playerId) => SFCore.GetPlayerName(playerId);
    public static int GetLocalPlayerId() => SFCore.GetLocalPlayerId();
    public static int? GetAimedPlayerId() => SFCore.GetAimedPlayerId();
    public static int? GetPlayerScore(int playerId) => SFCore.GetPlayerScore(playerId);
    public static void UpdateScoreAndPing() => SFCore.UpdateScoreAndPing();
    public static bool IsKeyDown(VK key) => SFCore.IsKeyDown(key);
    public static bool IsKeyPressed(VK key) => SFCore.IsKeyPressed(key);
    public static async Task<string> TakeScreenshotAsync()
    {
        var screensPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GTA San Andreas User Files",
            "SAMP",
            "screens"
        );
        var latestWriteTime = Directory.EnumerateFiles(screensPath).Select(File.GetLastWriteTime).Max();
        SFCore.TakeScreenshot();
        while (true)
        {
            // 1. Wait until a new file exists.
            var latestFile = Directory.EnumerateFiles(screensPath).Select(x => (Path: x, WriteTime: File.GetLastWriteTime(x))).MaxBy(x => x.WriteTime);
            if(latestFile.WriteTime != latestWriteTime)
            {
                // 2. Wait until it's writeable (in case SAMP might be finishing writing to it).
                try
                {
                    using var file = File.OpenWrite(latestFile.Path);
                }
                catch
                {
                    //SF.AddChatMessage("catch");
                    await Task.Yield();
                    continue;
                }
                return latestFile.Path;
            }
            await Task.Yield();
        }
    }
}