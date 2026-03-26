using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SFSharp;

public partial class DebugModule
{
    private void MapEndpoints(WebApplication app)
    {
        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path == "/ws" && ctx.WebSockets.IsWebSocketRequest)
            {
                await HandleWebSocket(ctx);
                return;
            }
            await next(ctx);
        });

        app.MapGet("/", () => Results.Content(DashboardHtml, "text/html"));

        app.MapGet("/api/config", () =>
        {
            var cfg = new ConfigDto(_captureEnabled, _captureIncoming, _captureOutgoing, _captureRpc, _capturePackets);
            return Results.Json(cfg, DebugJsonContext.Default.ConfigDto);
        });

        app.MapPost("/api/config", async (HttpContext ctx) =>
        {
            var cfg = await JsonSerializer.DeserializeAsync(ctx.Request.Body, DebugJsonContext.Default.ConfigDto);
            if (cfg is null) return Results.BadRequest();
            _captureEnabled = cfg.Capture;
            _captureIncoming = cfg.Incoming;
            _captureOutgoing = cfg.Outgoing;
            _captureRpc = cfg.Rpc;
            _capturePackets = cfg.Packets;
            UpdateDetails();
            BroadcastJson(new WsMessage<ConfigDto>("config", cfg), DebugJsonContext.Default.WsMessageConfigDto);
            return Results.Json(cfg, DebugJsonContext.Default.ConfigDto);
        });

        app.MapPost("/api/clear", () =>
        {
            while (_buffer.TryDequeue(out _)) { }
            Interlocked.Exchange(ref _totalInRpc, 0);
            Interlocked.Exchange(ref _totalOutRpc, 0);
            Interlocked.Exchange(ref _totalInPkt, 0);
            Interlocked.Exchange(ref _totalOutPkt, 0);
            BroadcastJson(new WsMessage<object?>("clear", null), DebugJsonContext.Default.WsMessageObject);
            return Results.Ok();
        });

        app.MapGet("/api/stats", () =>
        {
            var stats = BuildStats();
            return Results.Json(stats, DebugJsonContext.Default.StatsResponse);
        });
    }

    private async Task HandleWebSocket(HttpContext ctx)
    {
        var ws = await ctx.WebSockets.AcceptWebSocketAsync();
        string id = Guid.NewGuid().ToString("N");
        var client = new ClientState(ws);
        _clients[id] = client;

        try
        {
            var currentEntries = _buffer.ToArray();
            if (currentEntries.Length > 0)
            {
                var batchMsg = new WsMessage<TrafficEntry[]>("batch", currentEntries);
                var json = JsonSerializer.SerializeToUtf8Bytes(batchMsg, DebugJsonContext.Default.WsMessageTrafficEntryArray);
                await ws.SendAsync(json, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            var cfgMsg = new WsMessage<ConfigDto>("config", new ConfigDto(_captureEnabled, _captureIncoming, _captureOutgoing, _captureRpc, _capturePackets));
            var cfgJson = JsonSerializer.SerializeToUtf8Bytes(cfgMsg, DebugJsonContext.Default.WsMessageConfigDto);
            await ws.SendAsync(cfgJson, WebSocketMessageType.Text, true, CancellationToken.None);

            var statsMsg = new WsMessage<StatsResponse>("stats", BuildStats());
            var statsJson = JsonSerializer.SerializeToUtf8Bytes(statsMsg, DebugJsonContext.Default.WsMessageStatsResponse);
            await ws.SendAsync(statsJson, WebSocketMessageType.Text, true, CancellationToken.None);

            var buf = new byte[256];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buf, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }
        finally
        {
            _clients.TryRemove(id, out _);
            if (ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
    }

    private StatsResponse BuildStats()
    {
        var entries = _buffer.ToArray();
        var rpcCounts = new Dictionary<int, (string? Name, int Count)>();
        var pktCounts = new Dictionary<int, (string? Name, int Count)>();

        foreach (var e in entries)
        {
            var dict = e.Kind == TrafficKind.Rpc ? rpcCounts : pktCounts;
            if (dict.TryGetValue(e.Id, out var existing))
                dict[e.Id] = (existing.Name ?? e.Name, existing.Count + 1);
            else
                dict[e.Id] = (e.Name, 1);
        }

        return new StatsResponse(
            Volatile.Read(ref _totalInRpc),
            Volatile.Read(ref _totalOutRpc),
            Volatile.Read(ref _totalInPkt),
            Volatile.Read(ref _totalOutPkt),
            rpcCounts.OrderByDescending(kv => kv.Value.Count).Take(10)
                .Select(kv => new TopEntry(kv.Key, kv.Value.Name, kv.Value.Count)).ToArray(),
            pktCounts.OrderByDescending(kv => kv.Value.Count).Take(10)
                .Select(kv => new TopEntry(kv.Key, kv.Value.Name, kv.Value.Count)).ToArray());
    }
}
