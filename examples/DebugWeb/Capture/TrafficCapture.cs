using SFSharp.Abstractions;
using SFSharp.Abstractions.Network;
using SFSharp.Abstractions.Network.Protocol;

namespace SFSharp.Examples.DebugWeb.Capture;

internal sealed class TrafficCapture : IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];
    private CaptureSettings _settings;
    public TrafficStore Store { get; }
    public ISFPacketParsers? Parsers { get; private set; }
    public event Action<TrafficSummary>? Captured;

    public TrafficCapture(CaptureSettings settings)
    {
        _settings = settings;
        Store = new TrafficStore(settings);
    }

    public CaptureSettings Settings => Volatile.Read(ref _settings);
    public void Configure(CaptureSettings settings)
    {
        Store.Configure(settings);
        Volatile.Write(ref _settings, settings);
    }

    public void Subscribe(ISF sf)
    {
        Parsers = sf.Samp.PacketParsers;
        _subscriptions.Add(sf.Samp.Packets.ObserveSources(observation => Record(observation.Incoming ? "in" : "out", "packet",
            observation.PacketId, observation.Data, observation.DataBitLength, observation.PayloadBitOffset,
            observation.PayloadBitLength, observation.SubId, observation.SubId.HasValue, observation.Source, observation.Reconstructed)));
        for (int id = 0; id <= 255; id++)
        {
            int key = id;
            _subscriptions.Add(sf.Samp.Rpc.SubscribeIncoming(key, frame => Record("in", "rpc", frame.RpcId, frame.Data,
                frame.DataBitOffset + frame.DataBitLength, frame.DataBitOffset, frame.DataBitLength)));
            _subscriptions.Add(sf.Samp.Rpc.SubscribeOutgoing(key, frame => Record("out", "rpc", frame.RpcId, frame.Data,
                frame.DataBitLength, 0, frame.DataBitLength)));
            _subscriptions.Add(sf.Samp.Packets.SubscribeIncoming(key, Incoming));
            _subscriptions.Add(sf.Samp.Packets.SubscribeOutgoing(key, Outgoing));
        }
    }

    private void Incoming(IncomingPacketFrame frame) => Record("in", "packet", frame.PacketId, frame.Data,
        frame.DataBitLength, frame.PacketDataBitOffset, frame.PacketDataBitLength);
    private void Outgoing(OutgoingPacketFrame frame) => Record("out", "packet", frame.PacketId, frame.Data,
        frame.DataBitLength, 0, frame.DataBitLength);

    internal void Record(string direction, string kind, int id, ReadOnlyMemory<byte> data, int totalBits,
        int offset, int length, int? explicitSubId = null, bool control = false, string? source = null, bool reconstructed = false)
    {
        CaptureSettings settings = Settings;
        if (!settings.Capture || (direction == "in" ? !settings.Incoming : !settings.Outgoing)
            || (kind == "rpc" ? !settings.Rpc : !settings.Packets)) return;
        if (totalBits < 0 || totalBits > (long)data.Length * 8 || offset < 0 || length < 0
            || (long)offset + length > totalBits) return;

        int effectiveId = id;
        int packetOffset = 0;
        if (kind == "packet" && id == (int)RakNetPacketId.Timestamp
            && RakNetTimestampEnvelope.TryRead(data.Span, totalBits, out var envelope))
        {
            effectiveId = envelope.PacketId;
            packetOffset = RakNetTimestampEnvelope.PrefixByteLength * 8;
        }
        int? subId = explicitSubId;
        SFPacketMetadata? metadata = kind == "packet"
            ? Parsers?.Describe(effectiveId, direction == "in", data[(packetOffset / 8)..], totalBits - packetOffset) : null;
        if (metadata is not null)
        {
            if (metadata.HeaderBitLength < 0 || metadata.HeaderBitLength > totalBits - packetOffset) metadata = null;
        }
        if (metadata is not null)
        {
            subId = metadata.SubId ?? explicitSubId;
            if (source is null)
            {
                offset += metadata.HeaderBitLength;
                length -= metadata.HeaderBitLength;
            }
        }
        int[] ids = kind == "rpc" ? settings.RpcIds : settings.PacketIds;
        if (ids.Length > 0 && !ids.Contains(effectiveId)) return;
        if (settings.SubIds.Length > 0 && (!subId.HasValue || !settings.SubIds.Contains(subId.Value))) return;

        string name = metadata?.Name ?? (kind == "rpc" ? ((SampRpcId)id).ToString() : ((RakNetPacketId)effectiveId).ToString());
        TrafficSummary summary = new(0, direction, kind, id, effectiveId, subId, name, metadata?.Group,
            (totalBits + 7) / 8, Environment.TickCount64, metadata?.Category, source);
        TrafficSummary? stored = Store.Add(new(summary, data[..summary.DataBytes], totalBits, offset, length, control, reconstructed));
        if (stored is not null) Captured?.Invoke(stored);
    }

    public void Dispose()
    {
        foreach (IDisposable subscription in _subscriptions) subscription.Dispose();
        _subscriptions.Clear();
        Captured = null;
        Parsers = null;
        Store.Clear();
    }
}
