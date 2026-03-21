using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace SFSharp;

public partial class SFChat
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
}

