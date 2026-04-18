using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFSharp.Abstractions.Interop.RakNet.BitStream;
using SFSharp.Abstractions.Modules;
using SFSharp.Abstractions.Modules.Lifecycle;

namespace SFSharp.Examples.CefEmitor;

[SFModule(
    "example.cef-emitor",
    "CEF Emitor Example",
    Category = "Examples",
    Description = "Web UI on http://localhost:7778 that injects synthetic Arizona 220/17 InjectCode packets.",
    DefaultEnabled = false,
    ExecutionModel = ModuleExecutionModel.BackgroundWorker,
    RestartPolicy = ModuleRestartPolicy.Manual)]
public sealed class CefEmitorModule : ISFModule
{
    private const int WebPort = 7778;
    private const byte PacketId = 220;
    private const byte InjectCodeSubId = 17;
    private static readonly Encoding PacketStringEncoding;

    static CefEmitorModule()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        PacketStringEncoding = Encoding.GetEncoding(1251);
    }

    private IModuleContext Context => ((ISFModule)this).Context;
    private ILogger Log => ((ISFModule)this).Log;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string pluginDir = Path.GetDirectoryName(typeof(CefEmitorModule).Assembly.Location)
            ?? AppContext.BaseDirectory;
        string webRoot = Path.Combine(pluginDir, "wwwroot");

        if (!Directory.Exists(webRoot))
        {
            Log.LogError("CefEmitor: wwwroot directory not found at '{Path}'", webRoot);
            Context.SetStatusText("missing wwwroot");
            Context.SetDetail("error", $"wwwroot not found: {webRoot}");
            return;
        }

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ContentRootPath = pluginDir,
            WebRootPath = webRoot,
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://localhost:{WebPort}");

        await using WebApplication app = builder.Build();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.Map("/emit/injectcode", branch => branch.Run(HandleEmitInjectCodeHttpAsync));

        Context.SetDetail("url", $"http://localhost:{WebPort}/");
        Context.SetDetail("webRoot", webRoot);
        Context.SetStatusText($"listening on :{WebPort}");

        Log.LogInformation("CefEmitor: starting web UI on http://localhost:{Port}/", WebPort);
        Context.SF.Chat.Add($"{{58A6FF}}CefEmitor: {{FFFFFF}}http://localhost:{WebPort}/");

        await app.StartAsync(cancellationToken);
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }
    }

    private async Task HandleEmitInjectCodeHttpAsync(HttpContext http)
    {
        if (!HttpMethods.IsPost(http.Request.Method))
        {
            http.Response.Headers.Allow = "POST";
            await WriteJsonResponseAsync(
                http,
                StatusCodes.Status405MethodNotAllowed,
                ok: false,
                error: "method not allowed",
                cancellationToken: http.RequestAborted);
            return;
        }

        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(http.Request.Body, cancellationToken: http.RequestAborted);
        }
        catch (JsonException ex)
        {
            await WriteJsonResponseAsync(
                http,
                StatusCodes.Status400BadRequest,
                ok: false,
                error: $"invalid json: {ex.Message}",
                cancellationToken: http.RequestAborted);
            return;
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (!TryGetPropertyIgnoreCase(root, "code", out JsonElement codeElement) ||
                codeElement.ValueKind != JsonValueKind.String)
            {
                await WriteJsonResponseAsync(
                    http,
                    StatusCodes.Status400BadRequest,
                    ok: false,
                    error: "field 'code' is required",
                    cancellationToken: http.RequestAborted);
                return;
            }

            string code = codeElement.GetString()!;
            uint browserId = TryGetUInt32PropertyIgnoreCase(root, "browserId", out uint parsedBrowserId)
                ? parsedBrowserId
                : 0;
            uint requestId = TryGetUInt32PropertyIgnoreCase(root, "requestId", out uint parsedRequestId)
                ? parsedRequestId
                : 0;

            int sentBytes;
            bool ok;
            try
            {
                await Context.SwitchToMainThreadAsync();

                BitStreamWriter writer = BuildInjectCodePacket(browserId, code, requestId);
                sentBytes = writer.ByteLength;
                ok = Context.SF.Network.SimulateIncomingPacket(writer.AsSpan());
            }
            catch (ArgumentException ex)
            {
                await WriteJsonResponseAsync(
                    http,
                    StatusCodes.Status400BadRequest,
                    ok: false,
                    error: ex.Message,
                    cancellationToken: http.RequestAborted);
                return;
            }

            Context.IncrementCounter(ok ? "injected" : "injectFailed");
            Log.LogInformation(
                "CefEmitor: injected 220/17 browserId={BrowserId} codeLen={CodeLen} requestId={RequestId} ok={Ok}",
                browserId, sentBytes, requestId, ok);

            await WriteJsonResponseAsync(
                http,
                StatusCodes.Status200OK,
                ok: ok,
                sentBytes: sentBytes,
                cancellationToken: http.RequestAborted);
        }
    }

    private static BitStreamWriter BuildInjectCodePacket(uint browserId, string code, uint requestId)
    {
        byte[] codeBytes = PacketStringEncoding.GetBytes(code);
        if (codeBytes.Length > ushort.MaxValue)
        {
            throw new ArgumentException(
                $"code byte length {codeBytes.Length} exceeds u16 max of {ushort.MaxValue}");
        }

        BitStreamWriter writer = new(1 + 1 + sizeof(uint) + sizeof(ushort) + sizeof(byte) + codeBytes.Length + sizeof(uint));
        writer.WriteUInt8(PacketId);
        writer.WriteUInt8(InjectCodeSubId);
        writer.WriteUInt32(browserId);
        writer.WriteUInt16((ushort)codeBytes.Length);
        writer.WriteUInt8(0);
        writer.WriteBytes(codeBytes);
        writer.WriteUInt32(requestId);
        return writer;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement root, string name, out JsonElement value)
    {
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool TryGetUInt32PropertyIgnoreCase(JsonElement root, string name, out uint value)
    {
        if (TryGetPropertyIgnoreCase(root, name, out JsonElement element) &&
            element.ValueKind == JsonValueKind.Number &&
            element.TryGetUInt32(out uint parsed))
        {
            value = parsed;
            return true;
        }

        value = 0;
        return false;
    }

    private static async Task WriteJsonResponseAsync(
        HttpContext http,
        int statusCode,
        bool ok,
        int? sentBytes = null,
        string? error = null,
        CancellationToken cancellationToken = default)
    {
        http.Response.StatusCode = statusCode;
        http.Response.ContentType = "application/json; charset=utf-8";

        Utf8JsonWriter writer = new(http.Response.BodyWriter);
        writer.WriteStartObject();
        writer.WriteBoolean("ok", ok);
        if (sentBytes.HasValue)
        {
            writer.WriteNumber("sentBytes", sentBytes.Value);
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            writer.WriteString("error", error);
        }

        writer.WriteEndObject();
        await writer.FlushAsync(cancellationToken);
        writer.Dispose();
    }
}
