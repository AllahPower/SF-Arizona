namespace SFSharp.Runtime.Networking;

public sealed partial class SFPackets
{
    private readonly Lock _observationSync = new();
    private readonly HashSet<string> _sources = new(StringComparer.Ordinal);
    private readonly List<Action<SFPacketObservation>> _observers = [];

    public ISFPacketSource RegisterSource(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        lock (_observationSync)
            if (!_sources.Add(source)) throw new InvalidOperationException($"Packet source '{source}' is already registered.");
        return new PacketSource(this, source);
    }

    public IDisposable ObserveSources(Action<SFPacketObservation> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (_observationSync) _observers.Add(observer);
        return new NetworkSubscription(() => { lock (_observationSync) _observers.Remove(observer); });
    }

    private sealed class PacketSource(SFPackets owner, string source) : ISFPacketSource
    {
        private SFPackets? _owner = owner;

        public void Publish(bool incoming, int packetId, ReadOnlySpan<byte> data, int dataBitLength,
            int payloadBitOffset, int payloadBitLength, int? subId = null, bool reconstructed = false)
        {
            var target = Volatile.Read(ref _owner);
            if (target is null) return;
            if (packetId is < 0 or > 255 || dataBitLength < 0 || dataBitLength > (long)data.Length * 8
                || payloadBitOffset < 0 || payloadBitLength < 0 || (long)payloadBitOffset + payloadBitLength > dataBitLength)
                throw new ArgumentOutOfRangeException(nameof(dataBitLength));
            Action<SFPacketObservation>[] listeners;
            lock (target._observationSync) listeners = target._observers.ToArray();
            if (listeners.Length == 0) return;
            SFPacketObservation observation = new(source, incoming, packetId, data[..((dataBitLength + 7) / 8)].ToArray(),
                dataBitLength, payloadBitOffset, payloadBitLength, subId, reconstructed);
            foreach (var listener in listeners)
            {
                try { listener(observation); }
                catch (Exception exception) { SFLog.Error(exception, $"Packet source observer failed source={source}"); }
            }
        }

        public void Dispose()
        {
            var target = Interlocked.Exchange(ref _owner, null);
            if (target is not null) lock (target._observationSync) target._sources.Remove(source);
        }
    }
}
