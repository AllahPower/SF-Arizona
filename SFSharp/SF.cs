namespace SFSharp;

public static partial class SF
{
    public static string SFSharpDirectory => Path.Combine(Environment.CurrentDirectory, "SFSharp");

    public static void BeginInvoke(Action callback)
    {
        SFCore.PostToMainLoop(callback);
    }

    public static string? GetPlayerName(ushort playerId) => SFCore.GetPlayerName(playerId);
    public static ushort GetLocalPlayerId() => SFCore.GetLocalPlayerId();
    public static ushort? GetAimedPlayerId() => SFCore.GetAimedPlayerId();
    public static int? GetPlayerScore(ushort playerId) => SFCore.GetPlayerScore(playerId);
    public static void UpdateScoreAndPing() => SFCore.UpdateScoreAndPing();
}