namespace SFSharp.Abstractions.Arizona;

/// <summary>
/// Plugin-facing Arizona packet transport facade. Surface includes only copied payload frames and
/// sub-id routing, without exposing game-memory readers or parser internals.
/// </summary>
/// <remarks>Subscribe/Stream registration is thread-safe. Handlers fire on the main game thread.</remarks>
public interface ISFArizonaPackets
{
    IDisposable SubscribeIncoming(int subId, Action<IncomingArizonaPacketFrame> handler);
    IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncoming(int subId, CancellationToken token = default);

    IDisposable SubscribeIncomingEx(int subId, Action<IncomingArizonaPacketFrame> handler);
    IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncomingEx(int subId, CancellationToken token = default);

    IDisposable SubscribeOutgoing(int subId, Action<OutgoingArizonaPacketFrame> handler);
    IAsyncEnumerable<OutgoingArizonaPacketFrame> StreamOutgoing(int subId, CancellationToken token = default);

    IDisposable SubscribeOutgoingEx(int subId, Action<OutgoingArizonaPacketFrame> handler);
    IAsyncEnumerable<OutgoingArizonaPacketFrame> StreamOutgoingEx(int subId, CancellationToken token = default);

    IDisposable SubscribeIncomingAZVoice(int subId, Action<IncomingArizonaPacketFrame> handler);
    IAsyncEnumerable<IncomingArizonaPacketFrame> StreamIncomingAZVoice(int subId, CancellationToken token = default);

    IDisposable SubscribeIncomingAZVoiceData(Action<IncomingPacketFrame> handler);
    IAsyncEnumerable<IncomingPacketFrame> StreamIncomingAZVoiceData(CancellationToken token = default);

    IDisposable SubscribeOutgoingAZVoiceData(Action<OutgoingPacketFrame> handler);
    IAsyncEnumerable<OutgoingPacketFrame> StreamOutgoingAZVoiceData(CancellationToken token = default);
}
