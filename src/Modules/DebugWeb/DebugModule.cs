using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFSharp;
using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

[SFModule("debug-web", "DebugWeb",
    Category = "Debug",
    Description = "Web dashboard for network traffic inspection at http://localhost:7777",
    ExecutionModel = ModuleExecutionModel.BackgroundWorker,
    Order = 70)]
public partial class DebugModule : SFModuleBase
{
    private const int WebPort = 7777;
    private const int ServerBufferSize = 200;

    private readonly ConcurrentQueue<TrafficEntry> _buffer = new();
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
    private volatile bool _captureEnabled;
    private volatile bool _captureIncoming = true;
    private volatile bool _captureOutgoing = true;
    private volatile bool _captureRpc = true;
    private volatile bool _capturePackets = true;
    private int _totalInRpc, _totalOutRpc, _totalInPkt, _totalOutPkt;
    private long _entrySeq;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        List<IDisposable> subs = [];
        try
        {
            SubscribeAll(subs);
            Context.SetDetail("subscriptions", subs.Count.ToString());
            Context.SetDetail("url", $"http://localhost:{WebPort}/");

            var builder = WebApplication.CreateSlimBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls($"http://localhost:{WebPort}");

            var app = builder.Build();
            app.UseWebSockets();
            MapEndpoints(app);

            Log.LogInformation("Starting web server on http://localhost:{Port}/", WebPort);
            SF.Chat.Add($"{{58A6FF}}DebugWeb: {{FFFFFF}}http://localhost:{WebPort}/");

            await app.StartAsync(cancellationToken);

            try { await Task.Delay(Timeout.Infinite, cancellationToken); }
            catch (OperationCanceledException) { }

            await app.StopAsync();
        }
        finally
        {
            foreach (IDisposable sub in subs) sub.Dispose();
        }
    }

    private void SubscribeAll(List<IDisposable> subs)
    {
        foreach (ERpcId rpcId in Enum.GetValues<ERpcId>())
        {
            subs.Add(Context.RegisterDisposable(
                SF.Rpc.Subscribe(rpcId, args => OnIncomingRpc(args))));
            subs.Add(Context.RegisterDisposable(
                SF.Rpc.SubscribeOutgoing(rpcId, args => OnOutgoingRpc(args))));
        }

        foreach (EPacketId packetId in Enum.GetValues<EPacketId>())
        {
            if (packetId == EPacketId.AZVoice)
            {
                continue;
            }

            subs.Add(Context.RegisterDisposable(
                SF.Packets.SubscribeIncoming(packetId, args => OnIncomingPacket(args))));
            subs.Add(Context.RegisterDisposable(
                SF.Packets.SubscribeOutgoing(packetId, args => OnOutgoingPacket(args))));
        }

        foreach (EAZVoice subId in Enum.GetValues<EAZVoice>())
        {
            subs.Add(Context.RegisterDisposable(
                SF.Arizona.SubscribeIncomingAZVoice(subId, args => OnIncomingAZVoiceControl(args))));
        }

        subs.Add(Context.RegisterDisposable(
            SF.Arizona.SubscribeIncomingAZVoiceData(args => OnIncomingPacket(args))));
        subs.Add(Context.RegisterDisposable(
            SF.Arizona.SubscribeOutgoingAZVoiceData(args => OnOutgoingPacket(args))));
    }

    private void OnIncomingAZVoiceControl(IncomingArizonaPacketArgs args)
    {
        Interlocked.Increment(ref _totalInPkt);
        if (!_captureEnabled || !_captureIncoming || !_capturePackets) return;

        (string? name, string? detail, string? parsed) = DecodeIncomingAZVoiceControl(args);
        int dataByteLength = (args.PayloadBitOffset + args.PayloadBitLength + 7) / 8;
        Push(new TrafficEntry(0, TrafficDirection.Incoming, TrafficKind.Packet, args.EPacketId, name,
            parsed, detail, dataByteLength, Environment.TickCount64));
    }

    private void Push(TrafficEntry entry)
    {
        entry = entry with { Seq = Interlocked.Increment(ref _entrySeq) };
        _buffer.Enqueue(entry);
        while (_buffer.Count > ServerBufferSize) _buffer.TryDequeue(out _);

        if (_clients.IsEmpty) return;

        var msg = new WsMessage<TrafficEntry>("entry", entry);
        var json = JsonSerializer.SerializeToUtf8Bytes(msg, DebugJsonContext.Default.WsMessageTrafficEntry);
        BroadcastRaw(json);
    }

    private void BroadcastJson<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(value, typeInfo);
        BroadcastRaw(json);
    }

    private void BroadcastRaw(byte[] json)
    {
        foreach (var (id, ws) in _clients)
        {
            if (ws.State != WebSocketState.Open)
            {
                _clients.TryRemove(id, out _);
                continue;
            }
            _ = ws.SendAsync(json, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private void UpdateDetails()
    {
        Context.SetDetail("capture", _captureEnabled ? "on" : "off");
        Context.SetDetail("incoming", _captureIncoming ? "on" : "off");
        Context.SetDetail("outgoing", _captureOutgoing ? "on" : "off");
        Context.SetDetail("rpc", _captureRpc ? "on" : "off");
        Context.SetDetail("packets", _capturePackets ? "on" : "off");
    }
}
