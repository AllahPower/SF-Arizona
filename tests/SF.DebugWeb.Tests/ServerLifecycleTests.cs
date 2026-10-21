using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SFSharp.Examples.DebugWeb.Capture;
using SFSharp.Examples.DebugWeb.Transport;
using SFSharp.Abstractions;

namespace SF.DebugWeb.Tests;

public sealed class ServerLifecycleTests
{
    [Fact]
    public async Task ServerStopsWithConnectedClientAndCanRestartOnTheSamePort()
    {
        int port = AvailablePort();
        for (int attempt = 0; attempt < 2; attempt++)
        {
            using var stop = new CancellationTokenSource();
            using TrafficCapture capture = new(new() { Port = port, Capture = true });
            InlineMainThread main = new();
            DebugWebServer server = new(capture, main, _ => { }, NullLogger.Instance);
            string webRoot = FrontendRoot();
            Task running = server.RunAsync(webRoot, stop.Token);
            using HttpClient http = new(new SocketsHttpHandler { UseProxy = false }) { BaseAddress = new Uri($"http://localhost:{port}") };
            try
            {
                await ReadyAsync(http, running);
                Assert.Contains("SF / DebugWeb", await http.GetStringAsync("/", TestContext.Current.CancellationToken));
                using var foreignOrigin = new HttpRequestMessage(HttpMethod.Get, "/api/stats");
                foreignOrigin.Headers.Add("Origin", "https://example.com");
                using var rejected = await http.SendAsync(foreignOrigin, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.Forbidden, rejected.StatusCode);
                using var foreignHost = new HttpRequestMessage(HttpMethod.Get, "/api/stats");
                foreignHost.Headers.Host = "debug-web.example.com";
                using var rejectedHost = await http.SendAsync(foreignHost, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.Forbidden, rejectedHost.StatusCode);
                byte[] raw = [255, 1, 2];
                capture.Record("in", "packet", 255, raw, 21, 0, 21);
                byte[] download = await http.GetByteArrayAsync("/api/entries/1/raw", TestContext.Current.CancellationToken);
                Assert.Equal(raw, download);
                using var details = JsonDocument.Parse(await http.GetStringAsync("/api/entries/1", TestContext.Current.CancellationToken));
                Assert.Equal(21, details.RootElement.GetProperty("dataBitLength").GetInt32());
                byte[] chat = [202, 1, 0, 0, 0, (byte)'A'];
                capture.Record("in", "packet", 202, chat, chat.Length * 8, 0, chat.Length * 8);
                string chatJson = await http.GetStringAsync("/api/entries/2", TestContext.Current.CancellationToken);
                Assert.Contains("IncomingRconResponsePacket", chatJson);
                Assert.Contains("\"value\":\"A\"", chatJson);
                using var invalidSettings = new StringContent("{\"rawBudgetMiB\":-1}", System.Text.Encoding.UTF8, "application/json");
                using var invalidResponse = await http.PostAsync("/api/config", invalidSettings, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
                Assert.Equal(32, capture.Settings.RawBudgetMiB);
                using ClientWebSocket socket = new();
                socket.Options.Proxy = null;
                await socket.ConnectAsync(new Uri($"ws://localhost:{port}/ws"), TestContext.Current.CancellationToken);
                byte[] buffer = new byte[4096];
                var received = await socket.ReceiveAsync(buffer, TestContext.Current.CancellationToken);
                Assert.True(received.Count > 0);
                await stop.CancelAsync();
                await running.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            }
            finally { await stop.CancelAsync(); await running.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken); }
        }
    }

    [Fact]
    public void StoppedServerDoesNotRootCollectiblePluginAssembly()
    {
        WeakReference weak = RunCollectibleServer(AvailablePort(), FrontendRoot());
        for (int cycle = 0; cycle < 15 && weak.IsAlive; cycle++)
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Thread.Sleep(20);
        }
        Assert.False(weak.IsAlive, "Stopped HTTP server retained its collectible plugin load context.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RunCollectibleServer(int port, string webRoot)
    {
        var alc = new AssemblyLoadContext("DebugWeb unload test", isCollectible: true);
        Assembly assembly = alc.LoadFromAssemblyPath(typeof(DebugWebServer).Assembly.Location);
        Type settingsType = assembly.GetType(typeof(CaptureSettings).FullName!)!;
        object settings = Activator.CreateInstance(settingsType)!;
        settingsType.GetProperty("Port")!.SetValue(settings, port);
        object capture = Activator.CreateInstance(assembly.GetType(typeof(TrafficCapture).FullName!)!, settings)!;
        object invoker = new InlineMainThread();
        Type serverType = assembly.GetType(typeof(DebugWebServer).FullName!)!;
        object server = Activator.CreateInstance(serverType, capture, invoker, null, NullLogger.Instance)!;
        using CancellationTokenSource stop = new();
        Task running = (Task)serverType.GetMethod("RunAsync")!.Invoke(server, [webRoot, stop.Token])!;
        using HttpClient http = new(new SocketsHttpHandler { UseProxy = false }) { BaseAddress = new Uri($"http://localhost:{port}") };
        try { ReadyAsync(http, running).GetAwaiter().GetResult(); }
        finally
        {
            stop.Cancel();
            running.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            ((IDisposable)capture).Dispose();
            alc.Unload();
        }
        return new WeakReference(alc);
    }

    private static async Task ReadyAsync(HttpClient http, Task running)
    {
        for (int retry = 0; retry < 100; retry++)
        {
            if (running.IsCompleted) { await running; throw new InvalidOperationException("Server completed before listening."); }
            try { using var response = await http.GetAsync("/api/stats", TestContext.Current.CancellationToken); if (response.IsSuccessStatusCode) return; }
            catch (HttpRequestException) { }
            await Task.Delay(30, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("DebugWeb server did not start.");
    }

    private static int AvailablePort()
    {
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string FrontendRoot()
    {
        string root = Path.GetDirectoryName(typeof(DebugWebServer).Assembly.Location)!;
        string web = Path.Combine(root, "wwwroot");
        Assert.True(File.Exists(Path.Combine(web, "index.html")), web);
        return web;
    }

    private sealed class InlineMainThread : ISFMainThread
    {
        public bool IsMainThread => true;
        public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            action();
            return Task.CompletedTask;
        }
        public Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(action());
        }
    }
}
