using SFSharp.Abstractions.Network.Protocol;
using SFSharp.Abstractions.Network;
using SFSharp.Protocol.Packets.Catalog;
using SFSharp.Protocol.Packets.Parsing;
using SFSharp.Protocol.Rpc.Parsing;
using SFSharp.Protocol.Transport;

namespace SFSharp.Examples.DebugWeb.Capture;

internal sealed record ParsedTraffic(object? Value, string Parser, string? Error);

internal sealed class TrafficDecoder(ISFPacketParsers? extensions = null)
{
    private readonly PacketParserRegistry _packets = PacketParserCatalog.CreateDefaultRegistry();
    private readonly RpcParserRegistry _rpc = RpcParserCatalog.CreateDefaultRegistry();

    public unsafe ParsedTraffic Decode(CapturedTraffic entry)
    {
        fixed (byte* pointer = entry.Data.Span)
        {
            var summary = entry.Summary;
            if (summary.Kind == "rpc")
            {
                RpcParseResult result;
                bool success = summary.Direction == "in"
                    ? _rpc.TryParseIncoming(new IncomingRpcArgs(summary.Id, (nint)pointer, entry.PayloadBitOffset, entry.PayloadBitLength), out result)
                    : _rpc.TryParseOutgoing(new OutgoingRpcArgs(summary.Id, (nint)pointer, entry.PayloadBitLength), out result);
                return new(success ? result.Rpc : null, result.ParserName, success ? null : result.Error ?? result.FailureReason.ToString());
            }

            PacketParseResult packet;
            bool parsed;
            if (extensions is not null)
            {
                SFSharp.Abstractions.Parsing.IParsedPacket? value;
                if (summary.Source is not null)
                {
                    extensions.TryParseObservation(new(summary.Source, summary.Direction == "in", summary.Id, entry.Data,
                        entry.DataBitLength, entry.PayloadBitOffset, entry.PayloadBitLength, entry.Control ? summary.SubId : null,
                        entry.Reconstructed), out value);
                }
                else if (summary.Direction == "in")
                {
                    extensions.TryParseIncoming(new IncomingPacketFrame(summary.Id, entry.Data, entry.DataBitLength), out var incoming);
                    value = incoming;
                }
                else
                {
                    extensions.TryParseOutgoing(new OutgoingPacketFrame(summary.Id, entry.Data, entry.DataBitLength), out var outgoing);
                    value = outgoing;
                }
                if (value is not null) return new(value, value.Name, null);
            }
            int prefix = summary.Id == (int)RakNetPacketId.Timestamp && summary.EffectiveId != summary.Id ? 40 : 0;
            byte* start = pointer + prefix / 8;
            parsed = summary.Direction == "in"
                ? _packets.TryParseIncoming(new IncomingPacketArgs(summary.EffectiveId, (nint)start, entry.DataBitLength - prefix), out packet)
                : _packets.TryParseOutgoing(new OutgoingPacketArgs(summary.EffectiveId, (nint)start, entry.DataBitLength - prefix), out packet);
            return new(parsed ? packet.Packet : null, packet.ParserName, parsed ? null : packet.ErrorMessage ?? packet.FailureReason.ToString());
        }
    }
}
