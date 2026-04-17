namespace SFSharp.Abstractions.Ui;

/// <summary>
/// Plugin-facing contract for in-game chat. Concrete implementation lives in the host and is
/// reached through <see cref="ISF.Chat"/>.
/// </summary>
/// <remarks>
/// <see cref="Send"/> and both <c>Add</c> overloads are NOT thread-safe and must be invoked on
/// the main game thread (they touch native SA-MP chat state). <see cref="RegisterChatCommand"/>,
/// <see cref="StreamEntries"/> and <see cref="StreamServerEntries"/> are thread-safe for
/// registration; stream handlers fire on the main game thread.
/// </remarks>
public interface ISFChat
{
    /// <summary>
    /// Sends <paramref name="message"/> to the server. Lines starting with <c>/</c> are dispatched
    /// as chat commands, plain text is sent as a regular chat message.
    /// </summary>
    /// <remarks>Main-thread only.</remarks>
    void Send(string message);

    /// <summary>
    /// Appends a line to the local chat window without sending it to the server. Intended for
    /// module UI output.
    /// </summary>
    /// <param name="text">Main line text. SAMP colour tags <c>{RRGGBB}</c> are honoured.</param>
    /// <param name="textColor">ARGB colour applied to the prefix-free portion of the line.</param>
    /// <param name="prefix">Optional prefix rendered in a separate colour.</param>
    /// <param name="prefixColor">ARGB colour applied to <paramref name="prefix"/>.</param>
    /// <remarks>Main-thread only. Legacy uint overload; prefer <see cref="Add(string, SFColor, string?, SFColor?)"/>.</remarks>
    void Add(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA);

    /// <summary>
    /// Appends a line to the local chat window using strongly-typed <see cref="SFColor"/> values.
    /// </summary>
    /// <param name="text">Main line text. SAMP colour tags <c>{RRGGBB}</c> are honoured.</param>
    /// <param name="textColor">Colour applied to the prefix-free portion of the line.</param>
    /// <param name="prefix">Optional prefix rendered in a separate colour.</param>
    /// <param name="prefixColor">Colour for <paramref name="prefix"/>. Falls back to <paramref name="textColor"/> when <see langword="null"/>.</param>
    /// <remarks>Main-thread only.</remarks>
    void Add(string text, SFColor textColor, string? prefix = null, SFColor? prefixColor = null);

    /// <summary>
    /// Registers a chat command handler. The callback is invoked on the main thread when the user
    /// types <c>/<paramref name="command"/></c> in chat. Dispose the returned handle to remove the
    /// registration.
    /// </summary>
    /// <param name="command">Command token without the leading slash.</param>
    /// <param name="commandCallback">Handler receiving the tail of the line after the command, or <see langword="null"/> when the user typed the bare command.</param>
    /// <remarks>Thread-safe.</remarks>
    IDisposable RegisterChatCommand(string command, Action<string?> commandCallback);

    /// <summary>
    /// Streams every chat entry appended to the local chat window, including server messages and
    /// lines added by other modules.
    /// </summary>
    /// <remarks>Registration is thread-safe. Entries are yielded on the main game thread.</remarks>
    IAsyncEnumerable<ChatEntry> StreamEntries(CancellationToken token = default);

    /// <summary>
    /// Streams only server-originated chat entries (chat RPC and client-message RPC), tagged with
    /// the source kind.
    /// </summary>
    /// <remarks>Registration is thread-safe. Entries are yielded on the main game thread.</remarks>
    IAsyncEnumerable<ServerChatEntry> StreamServerEntries(CancellationToken token = default);
}
