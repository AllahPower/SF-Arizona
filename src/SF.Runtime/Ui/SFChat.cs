namespace SFSharp.Runtime.Ui;

public partial class SFChat(ExceptionReporter exceptions, ISFMainThread mainThread) : ISFChat
{
    private void RequireThread()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP chat operations require the game thread.");
    }

    public void Send(string message)
    {
        RequireThread();
        if (message.StartsWith('/'))
        {
            CInput.Instance.Send(message);
            return;
        }

        CLocalPlayer.Instance.Chat(message);
    }

    // Main-thread only (CChat.Instance touches native SA-MP chat state).
    public void Add(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA)
    {
        RequireThread();
        CChat.Instance.AddEntry(EntryType.Debug, text, prefix, textColor, prefixColor);
    }

    // Main-thread only.
    public void Add(string text, SFColor textColor, string? prefix = null, SFColor? prefixColor = null)
    {
        RequireThread();
        CChat.Instance.AddEntry(EntryType.Debug, text, prefix, textColor.Argb, (prefixColor ?? textColor).Argb);
    }

    [Obsolete]
    public void AddDirect(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA, EntryType type = EntryType.Debug)
    {
        RequireThread();
        CChat.DirectWriteEntry(type, text, prefix, textColor, prefixColor);
        PublishLocalChatEntry(new ChatEntry(type, text, prefix, textColor, prefixColor));
    }

    [Obsolete]
    public NativeChatEntry[] GetHistory(int count = SampOffsets.CChat.EntryCount)
    {
        RequireThread();
        if (!CChat.IsAvailable)
        {
            return [];
        }

        count = Math.Clamp(count, 0, SampOffsets.CChat.EntryCount);
        if (count == 0)
        {
            return [];
        }

        var entries = new NativeChatEntry[count];
        int offset = SampOffsets.CChat.EntryCount - count;
        for (int i = 0; i < count; i++)
        {
            entries[i] = CChat.ReadEntry(offset + i);
        }

        return entries;
    }
}

