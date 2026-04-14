namespace SFSharp;

/// <summary>
/// Plugin-facing contract for in-game chat. Concrete implementation lives in the host and is
/// reached through <see cref="ISF.Chat"/>.
/// </summary>
public interface ISFChat
{
    /// <summary>
    /// Sends <paramref name="message"/> to the server. Lines starting with <c>/</c> are dispatched
    /// as chat commands, plain text is sent as a regular chat message.
    /// </summary>
    void Send(string message);

    /// <summary>
    /// Appends a line to the local chat window without sending it to the server. Intended for
    /// module UI output.
    /// </summary>
    /// <param name="text">Main line text. SAMP colour tags <c>{RRGGBB}</c> are honoured.</param>
    /// <param name="textColor">ARGB colour applied to the prefix-free portion of the line.</param>
    /// <param name="prefix">Optional prefix rendered in a separate colour.</param>
    /// <param name="prefixColor">ARGB colour applied to <paramref name="prefix"/>.</param>
    void Add(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA);

    /// <summary>
    /// Registers a chat command handler. The callback is invoked on the main thread when the user
    /// types <c>/<paramref name="command"/></c> in chat. Dispose the returned handle to remove the
    /// registration.
    /// </summary>
    /// <param name="command">Command token without the leading slash.</param>
    /// <param name="commandCallback">Handler receiving the tail of the line after the command, or <see langword="null"/> when the user typed the bare command.</param>
    IDisposable RegisterChatCommand(string command, Action<string?> commandCallback);
}
