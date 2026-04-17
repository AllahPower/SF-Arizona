using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFSharp.Abstractions.Interop.RakNet;
using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Modules;

[SFModule("debug-web", "DebugWeb",
    Category = "Debug",
    Description = "Web dashboard for network traffic inspection at http://localhost:7777",
    ExecutionModel = ModuleExecutionModel.BackgroundWorker,
    Order = 70)]
public partial class DebugModule : SFModuleBase
{
    private const int WebPort = 7777;
    private const int ServerBufferSize = 200;
    private const int BroadcastIntervalMs = 50;

    private readonly ConcurrentQueue<TrafficEntry> _buffer = new();
    private readonly ConcurrentDictionary<string, DebugClientSession> _clients = new();
    private readonly Channel<TrafficEntry> _broadcastChannel = Channel.CreateBounded<TrafficEntry>(
        new BoundedChannelOptions(8192) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });
    private volatile bool _captureEnabled;
    private volatile bool _captureIncoming = true;
    private volatile bool _captureOutgoing = true;
    private volatile bool _captureRpc = true;
    private volatile bool _capturePackets = true;
    private int _totalInRpc, _totalOutRpc, _totalInPkt, _totalOutPkt;
    private long _entrySeq;

    private sealed record PacketViewState(
        string SearchText,
        bool IsPaused,
        bool AutoScroll,
        IReadOnlyDictionary<string, string> IdFilters)
    {
        public static PacketViewState Default => new("", false, true, new Dictionary<string, string>());
    }

    private sealed record WorldViewState(
        string Section,
        string? SearchText,
        bool StreamZoneOnly)
    {
        public static WorldViewState Default => new("overview", null, false);
    }

    private sealed class DebugClientSession
    {
        public WebSocket Socket { get; }
        public SemaphoreSlim SendLock { get; } = new(1, 1);
        public volatile PacketViewState PacketView = PacketViewState.Default;
        public volatile WorldViewState WorldView = WorldViewState.Default;

        public DebugClientSession(WebSocket socket) => Socket = socket;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!WebDebuggerAssetsExist)
        {
            string missingTarget = WebDebuggerIndexPath;
            Context.SetDetail("status", "missing web assets");
            Context.SetDetail("path", missingTarget);
            Log.LogWarning("DebugWeb assets are missing: {Path}", missingTarget);
            SF.Chat.Add($"{{FF6B6B}}DebugWeb: {{FFFFFF}}missing web assets: {missingTarget}");
            return;
        }

        List<IDisposable> subs = [];
        try
        {
            SubscribeAll(subs);
            Context.SetDetail("subscriptions", subs.Count.ToString());
            Context.SetDetail("url", $"http://localhost:{WebPort}/");
            Context.SetDetail("assetsRoot", Context.Assets.Root);
            Context.SetDetail("webRoot", WebDebuggerWwwRootPath);

            var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
            {
                ContentRootPath = Context.Assets.Root,
                WebRootPath = WebDebuggerWwwRootPath,
                ApplicationName = typeof(DebugModule).Assembly.GetName().Name
            });
            builder.Logging.ClearProviders();
            builder.WebHost.UseUrls($"http://localhost:{WebPort}");

            var app = builder.Build();
            app.UseWebSockets();
            MapEndpoints(app);

            Log.LogInformation("Starting web server on http://localhost:{Port}/", WebPort);
            SF.Chat.Add($"{{58A6FF}}DebugWeb: {{FFFFFF}}http://localhost:{WebPort}/");

            await app.StartAsync(cancellationToken);

            _ = Context.RunBackground(() => BroadcastLoopAsync(cancellationToken));
            _ = Context.RunBackground(() => BroadcastWorldUpdatesLoopAsync(cancellationToken));

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

        _broadcastChannel.Writer.TryWrite(entry);
    }

    private async Task BroadcastLoopAsync(CancellationToken ct)
    {
        var reader = _broadcastChannel.Reader;
        var batch = new List<TrafficEntry>(128);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await reader.WaitToReadAsync(ct);

                batch.Clear();
                while (reader.TryRead(out var entry))
                {
                    batch.Add(entry);
                }

                if (batch.Count == 0 || _clients.IsEmpty) continue;

                byte[] json;
                if (batch.Count == 1)
                {
                    var msg = new WsMessage<TrafficEntry>("entry", batch[0]);
                    json = JsonSerializer.SerializeToUtf8Bytes(msg, DebugJsonContext.Default.WsMessageTrafficEntry);
                }
                else
                {
                    var msg = new WsMessage<TrafficEntry[]>("batch", batch.ToArray());
                    json = JsonSerializer.SerializeToUtf8Bytes(msg, DebugJsonContext.Default.WsMessageTrafficEntryArray);
                }

                await BroadcastRawAsync(json);

                // throttle: don't spin faster than BroadcastIntervalMs
                await Task.Delay(BroadcastIntervalMs, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                SFLog.Error($"BroadcastLoop error: {ex.Message}");
            }
        }
    }

    private async Task BroadcastRawAsync(byte[] json)
    {
        foreach (var (id, client) in _clients)
        {
            if (client.Socket.State != WebSocketState.Open)
            {
                _clients.TryRemove(id, out _);
                continue;
            }

            await SendRawAsync(client, json, id);
        }
    }

    private async Task SendRawAsync(DebugClientSession client, byte[] json, string? clientId = null)
    {
        if (!client.SendLock.Wait(0))
        {
            return;
        }

        try
        {
            await client.Socket.SendAsync(json, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch
        {
            if (clientId is not null)
            {
                _clients.TryRemove(clientId, out _);
                return;
            }

            foreach (var pair in _clients)
            {
                if (ReferenceEquals(pair.Value, client))
                {
                    _clients.TryRemove(pair.Key, out _);
                    break;
                }
            }
        }
        finally
        {
            client.SendLock.Release();
        }
    }

    private void BroadcastJson<T>(T value, JsonTypeInfo<T> typeInfo)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(value, typeInfo);
        SFBootstrap.ObserveTask(BroadcastRawAsync(json), "DebugWeb.BroadcastRawAsync");
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
