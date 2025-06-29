namespace SFSharp;

public static class SFDebug
{
    private static Queue<string> _messages = new();
    public static void Log(SFChatEntry entry)
    {
        _messages.Enqueue($"{{AAAAAA}}[{TimeOnly.FromDateTime(DateTime.Now):T}] {{FFFFFF}}{entry.Text}");
        if (_messages.Count > 50) _messages.Dequeue();
    }

    public static void ShowDialog()
    {
        _ = SF.Dialog.ShowMessage("SFDebug", string.Join("\r\n", _messages));
    }
}