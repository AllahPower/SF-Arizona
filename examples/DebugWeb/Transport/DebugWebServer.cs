using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SFSharp.Examples.DebugWeb.Capture;
using SFSharp.Abstractions;

namespace SFSharp.Examples.DebugWeb.Transport;

internal sealed class DebugWebServer(TrafficCapture capture, ISFMainThread mainThread,
    Action<CaptureSettings> persist, ILogger log)
{
    private sealed class Client(WebSocket socket)
    {
        public WebSocket Socket { get; } = socket;
        public DebugWebSocketSender Sender { get; } = new(socket);
    }

    private readonly TrafficDecoder _decoder = new(capture.Parsers);
    private readonly ConcurrentDictionary<Guid, Client> _clients = new();
    private readonly Channel<TrafficSummary> _broadcast = Channel.CreateBounded<TrafficSummary>(
        new BoundedChannelOptions(8192) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });
    private long _dropped;
    private int _port;

    private void Enqueue(TrafficSummary entry)
    {
        if (_clients.IsEmpty) return;
        if (!_broadcast.Writer.TryWrite(entry)) Interlocked.Increment(ref _dropped);
    }

    internal async Task<TrafficDetails?> InspectAsync(long seq, CancellationToken cancellationToken)
    {
        CapturedTraffic? entry = capture.Store.Find(seq);
        if (entry is null) return null;
        ParsedTraffic parsed = await mainThread.InvokeAsync(() => _decoder.Decode(entry), cancellationToken);
        InspectionNode? tree = parsed.Value is null ? null : InspectionProjector.Project(parsed.Value);
        return new(capture.Store.Session, entry.Summary, tree, parsed.Parser, parsed.Error,
            entry.DataBitLength, entry.PayloadBitOffset, entry.PayloadBitLength, entry.Data.ToArray(), entry.Reconstructed);
    }

    public async Task RunAsync(string webRoot, CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.Combine(webRoot, "index.html"))) throw new DirectoryNotFoundException($"DebugWeb frontend is missing: {webRoot}");
        _port = capture.Settings.Port;
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ContentRootPath = webRoot, WebRootPath = webRoot });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://localhost:{_port}");
        await using WebApplication app = builder.Build();
        using var files = new PhysicalFileProvider(webRoot);
        app.Use(async (http, next) =>
        {
            string host = http.Request.Host.Host;
            string origin = http.Request.Headers.Origin.ToString();
            bool loopback = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || (IPAddress.TryParse(host.Trim('[', ']'), out var ip) && IPAddress.IsLoopback(ip));
            if (!loopback || (origin.Length > 0 && (!Uri.TryCreate(origin, UriKind.Absolute, out var source)
                || !source.IsLoopback || source.Scheme != "http" || source.Port != _port)))
            {
                http.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            await next(http);
        });
        app.UseWebSockets();
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = files });

        app.MapGet("/api/config", () => Json(capture.Settings, CaptureJsonContext.Default.CaptureSettings));
        app.MapPost("/api/config", async (HttpContext http) =>
        {
            CaptureSettings? settings;
            try { settings = await JsonSerializer.DeserializeAsync(http.Request.Body, CaptureJsonContext.Default.CaptureSettings, http.RequestAborted); }
            catch (JsonException ex) { return Error(ex.Message, 400); }
            if (settings is null) return Error("Settings are required.", 400);
            if (settings.Validate() is string error) return Error(error, 400);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, http.RequestAborted);
            await mainThread.InvokeAsync(() => { persist(settings); capture.Configure(settings); return true; }, linked.Token);
            return Json(settings, CaptureJsonContext.Default.CaptureSettings);
        });
        app.MapGet("/api/stats", () => Json(capture.Store.Stats(Interlocked.Read(ref _dropped), _port), CaptureJsonContext.Default.CaptureStats));
        app.MapGet("/api/entries", () => Json(new TrafficSnapshot(capture.Store.Session, capture.Store.Snapshot().Select(e => e.Summary).ToArray()), CaptureJsonContext.Default.TrafficSnapshot));
        app.MapGet("/api/entries/{seq:long}", async (HttpContext http, long seq) =>
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, http.RequestAborted);
            TrafficDetails? details = await InspectAsync(seq, linked.Token);
            return details is null ? Error("Entry was evicted or cleared.", 410) : Json(details, CaptureJsonContext.Default.TrafficDetails);
        });
        app.MapGet("/api/entries/{seq:long}/raw", (long seq) =>
        {
            CapturedTraffic? entry = capture.Store.Find(seq);
            return entry is null ? Error("Entry was evicted or cleared.", 410)
                : Results.File(entry.Data.ToArray(), "application/octet-stream", $"packet-{seq}.bin");
        });
        app.MapGet("/api/export", () => Results.File(JsonSerializer.SerializeToUtf8Bytes(
            new CaptureExport(2, capture.Store.Session, capture.Store.Snapshot()), CaptureJsonContext.Default.CaptureExport),
            "application/json", $"debug-web-{capture.Store.Session}.json"));
        app.MapPost("/api/clear", () => { capture.Store.Clear(); Interlocked.Exchange(ref _dropped, 0); return Json(capture.Store.Stats(0, _port), CaptureJsonContext.Default.CaptureStats); });
        app.Map("/ws", async (HttpContext http) =>
        {
            if (!http.WebSockets.IsWebSocketRequest) { http.Response.StatusCode = 400; return; }
            using WebSocket socket = await http.WebSockets.AcceptWebSocketAsync();
            Guid id = Guid.NewGuid();
            Client client = new(socket);
            _clients[id] = client;
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, http.RequestAborted);
            using var initialSend = CancellationTokenSource.CreateLinkedTokenSource(linked.Token);
            initialSend.CancelAfter(TimeSpan.FromSeconds(2));
            try
            {
                await client.Sender.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new TrafficSnapshot(capture.Store.Session,
                    capture.Store.Snapshot().Select(e => e.Summary).ToArray()), CaptureJsonContext.Default.TrafficSnapshot), initialSend.Token);
                byte[] buffer = new byte[1024];
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer, linked.Token);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                }
            }
            catch (OperationCanceledException) when (linked.IsCancellationRequested || initialSend.IsCancellationRequested) { }
            catch (WebSocketException) { }
            finally { _clients.TryRemove(id, out _); socket.Abort(); }
        });

        capture.Captured += Enqueue;
        Task? broadcast = null;
        try
        {
            await app.StartAsync(cancellationToken);
            log.LogInformation("DebugWeb listening at http://localhost:{Port}/", _port);
            broadcast = BroadcastAsync(cancellationToken);
            await Task.WhenAny(broadcast, Task.Delay(Timeout.Infinite, cancellationToken));
            await broadcast;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            capture.Captured -= Enqueue;
            _broadcast.Writer.TryComplete();
            foreach (Client client in _clients.Values) client.Socket.Abort();
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await app.StopAsync(stop.Token);
            if (broadcast is not null) await broadcast;
            _clients.Clear();
        }
    }

    private async Task BroadcastAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _broadcast.Reader.WaitToReadAsync(cancellationToken))
            {
                List<TrafficSummary> batch = [];
                while (batch.Count < 1024 && _broadcast.Reader.TryRead(out var entry)) batch.Add(entry);
                byte[] json = JsonSerializer.SerializeToUtf8Bytes(new TrafficSnapshot(capture.Store.Session, batch.ToArray()), CaptureJsonContext.Default.TrafficSnapshot);
                await Task.WhenAll(_clients.Select(pair => SendAsync(pair.Key, pair.Value, json, cancellationToken)));
                await Task.Delay(50, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private async Task SendAsync(Guid id, Client client, byte[] json, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(2));
        try { await client.Sender.SendAsync(json, timeout.Token); }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException or ObjectDisposedException)
        {
            _clients.TryRemove(id, out _);
            client.Socket.Abort();
        }
    }

    private static IResult Json<T>(T value, JsonTypeInfo<T> info) => Results.Bytes(JsonSerializer.SerializeToUtf8Bytes(value, info), "application/json");
    private static IResult Error(string error, int status) => Results.Json(new ApiError(error), CaptureJsonContext.Default.ApiError, statusCode: status);
}
