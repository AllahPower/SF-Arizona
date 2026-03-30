using System.Net.WebSockets;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SFSharp;

public partial class DebugModule
{
    private void MapEndpoints(WebApplication app)
    {
        Directory.CreateDirectory(WebDebuggerWwwRootPath);

        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path == "/ws" && ctx.WebSockets.IsWebSocketRequest)
            {
                await HandleWebSocket(ctx);
                return;
            }
            await next(ctx);
        });

        var webRootProvider = new PhysicalFileProvider(WebDebuggerWwwRootPath);
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = webRootProvider,
            DefaultFileNames = { "index.html" }
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = webRootProvider
        });

        app.MapGet("/", async (HttpContext ctx) =>
        {
            if (!File.Exists(WebDebuggerIndexPath))
            {
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "text/plain; charset=utf-8";
                await ctx.Response.WriteAsync($"Debug web root is missing: {WebDebuggerIndexPath}");
                return;
            }

            ctx.Response.ContentType = "text/html; charset=utf-8";
            await ctx.Response.SendFileAsync(WebDebuggerIndexPath);
        });

        app.MapGet("/api/config", () =>
        {
            var cfg = new DebugServerSettingsDto(_captureEnabled, _captureIncoming, _captureOutgoing, _captureRpc, _capturePackets);
            return Results.Json(cfg, DebugJsonContext.Default.DebugServerSettingsDto);
        });

        app.MapPost("/api/config", async (HttpContext ctx) =>
        {
            var cfg = await JsonSerializer.DeserializeAsync(ctx.Request.Body, DebugJsonContext.Default.DebugServerSettingsDto);
            if (cfg is null) return Results.BadRequest();
            _captureEnabled = cfg.Capture;
            _captureIncoming = cfg.Incoming;
            _captureOutgoing = cfg.Outgoing;
            _captureRpc = cfg.Rpc;
            _capturePackets = cfg.Packets;
            UpdateDetails();
            BroadcastJson(new WsMessage<DebugServerSettingsDto>("server-settings", cfg), DebugJsonContext.Default.WsMessageDebugServerSettingsDto);
            return Results.Json(cfg, DebugJsonContext.Default.DebugServerSettingsDto);
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
        var client = new DebugClientSession(ws);
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

            var cfgMsg = new WsMessage<DebugServerSettingsDto>("server-settings", new DebugServerSettingsDto(_captureEnabled, _captureIncoming, _captureOutgoing, _captureRpc, _capturePackets));
            var cfgJson = JsonSerializer.SerializeToUtf8Bytes(cfgMsg, DebugJsonContext.Default.WsMessageDebugServerSettingsDto);
            await ws.SendAsync(cfgJson, WebSocketMessageType.Text, true, CancellationToken.None);

            var statsMsg = new WsMessage<StatsResponse>("stats", BuildStats());
            var statsJson = JsonSerializer.SerializeToUtf8Bytes(statsMsg, DebugJsonContext.Default.WsMessageStatsResponse);
            await ws.SendAsync(statsJson, WebSocketMessageType.Text, true, CancellationToken.None);

            var worldSnapshot = await CaptureWorldSnapshotOnMainThreadAsync(client.WorldView);
            var worldMsg = new WsMessage<WorldSnapshotDto>("world", worldSnapshot);
            var worldJson = JsonSerializer.SerializeToUtf8Bytes(worldMsg, DebugJsonContext.Default.WsMessageWorldSnapshotDto);
            await ws.SendAsync(worldJson, WebSocketMessageType.Text, true, CancellationToken.None);

            var buf = new byte[2048];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buf, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                        ApplyClientSessionMessage(client, buf.AsSpan(0, result.Count));
                        await SendWorldSnapshotAsync(client);
                    }
                }
        }
        finally
        {
            _clients.TryRemove(id, out _);
            if (ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }
    }

    private static void ApplyClientSessionMessage(DebugClientSession client, ReadOnlySpan<byte> payload)
    {
        using JsonDocument doc = JsonDocument.Parse(payload.ToArray());
        JsonElement root = doc.RootElement;
        if (!root.TryGetProperty("type", out JsonElement typeNode))
        {
            return;
        }

        string? messageType = typeNode.GetString();
        if (!root.TryGetProperty("data", out JsonElement dataNode) || dataNode.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        switch (messageType)
        {
            case "world-view:update":
                client.WorldView = ParseWorldViewState(dataNode);
                break;
            case "packets-view:update":
                client.PacketView = ParsePacketViewState(dataNode);
                break;
        }
    }

    private static WorldViewState ParseWorldViewState(JsonElement dataNode)
    {
        string section = NormalizeWorldSectionKey(dataNode.TryGetProperty("section", out JsonElement sectionNode)
            ? sectionNode.GetString()
            : dataNode.TryGetProperty("pool", out JsonElement poolNode) ? poolNode.GetString() : null);
        string? searchText = NormalizeWorldQueryText(dataNode.TryGetProperty("search", out JsonElement searchNode) ? searchNode.GetString() : null);
        bool streamZoneOnly = dataNode.TryGetProperty("streamZone", out JsonElement streamNode) && streamNode.ValueKind == JsonValueKind.True;
        return new WorldViewState(section, searchText, streamZoneOnly);
    }

    private static PacketViewState ParsePacketViewState(JsonElement dataNode)
    {
        string searchText = dataNode.TryGetProperty("search", out JsonElement searchNode) ? searchNode.GetString() ?? string.Empty : string.Empty;
        bool isPaused = dataNode.TryGetProperty("paused", out JsonElement pausedNode) && pausedNode.ValueKind == JsonValueKind.True;
        bool autoScroll = !dataNode.TryGetProperty("autoScroll", out JsonElement autoScrollNode) || autoScrollNode.ValueKind == JsonValueKind.True;
        Dictionary<string, string> filters = [];
        if (dataNode.TryGetProperty("idFilters", out JsonElement filtersNode) && filtersNode.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement filterNode in filtersNode.EnumerateArray())
            {
                if (!filterNode.TryGetProperty("key", out JsonElement keyNode) || keyNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (!filterNode.TryGetProperty("mode", out JsonElement modeNode) || modeNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                string? key = keyNode.GetString();
                string? mode = modeNode.GetString();
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(mode))
                {
                    continue;
                }

                filters[key] = mode;
            }
        }

        return new PacketViewState(searchText, isPaused, autoScroll, filters);
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
