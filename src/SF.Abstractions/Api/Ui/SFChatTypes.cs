using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

/// <summary>Native SA-MP chat entry kind. Values mirror the in-game engine.</summary>
public enum EntryType : int
{
    None = 0,
    Chat = 2,
    Info = 4,
    Debug = 8
}

/// <summary>Source kind of a server-originated chat entry.</summary>
public enum ServerChatKind
{
    Chat,
    ClientMessage
}

/// <summary>Immutable chat entry captured from the local chat window.</summary>
public record ChatEntry(EntryType Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

/// <summary>Immutable chat entry produced by the server (chat or client message RPC).</summary>
public record ServerChatEntry(ServerChatKind Kind, ERpcId ERpcId, ChatEntry Entry)
{
    public EntryType Type => Entry.Type;
    public string? Text => Entry.Text;
    public string? Prefix => Entry.Prefix;
    public uint TextColor => Entry.TextColor;
    public uint PrefixColor => Entry.PrefixColor;
}
