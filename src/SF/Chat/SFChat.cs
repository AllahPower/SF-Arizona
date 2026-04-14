using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace SFSharp;

public partial class SFChat : ISFChat
{
    public void Send(string message)
    {
        if (message.StartsWith('/'))
        {
            CInput.Instance.Send(message);
            return;
        }

        CLocalPlayer.Instance.Chat(message);
    }

    public void Add(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA)
    {
        CChat.Instance.AddEntry(EntryType.Debug, text, prefix, textColor, prefixColor);
    }

    [Obsolete]
    public void AddDirect(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA, EntryType type = EntryType.Debug)
    {
        CChat.DirectWriteEntry(type, text, prefix, textColor, prefixColor);
        PublishLocalChatEntry(new ChatEntry(type, text, prefix, textColor, prefixColor));
    }

    [Obsolete]
    public NativeChatEntry[] GetHistory(int count = SampOffsets.CChat.EntryCount)
    {
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

