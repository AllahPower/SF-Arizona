using Microsoft.Extensions.Logging;
using SFSharp.Abstractions.Modules;
using SFSharp.Abstractions.Modules.Lifecycle;
using SFSharp.Examples.DebugWeb.Capture;
using SFSharp.Examples.DebugWeb.Transport;

namespace SFSharp.Examples.DebugWeb;

[SFModule("debug-web", "DebugWeb", Category = "Debug", Description = "Packet and RPC inspector at http://localhost:7777/",
    DefaultEnabled = false, ExecutionModel = ModuleExecutionModel.MainThread, RestartPolicy = ModuleRestartPolicy.Manual, Order = 70)]
public sealed class DebugWebModule : ISFModule
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        IModuleContext context = ((ISFModule)this).Context;
        SynchronizationContext gameThread = SynchronizationContext.Current
            ?? throw new InvalidOperationException("DebugWeb must start on the game synchronization context.");
        CaptureSettings settings = context.Config.Load(CaptureJsonContext.Default.CaptureSettings, () => new CaptureSettings()) with { Capture = false };
        if (settings.Validate() is string error) throw new InvalidDataException(error);
        using TrafficCapture capture = new(settings);
        capture.Subscribe(context.SF);
        MainThreadInvoker invoker = new(gameThread);
        DebugWebServer server = new(capture, invoker,
            changed => context.Config.Save(CaptureJsonContext.Default.CaptureSettings, changed with { Capture = false }), context.Log);
        string pluginRoot = Path.GetDirectoryName(typeof(DebugWebModule).Assembly.Location)
            ?? throw new InvalidOperationException("DebugWeb assembly has no installation directory.");
        string webRoot = Path.Combine(pluginRoot, "wwwroot");
        context.SetDetail("url", $"http://localhost:{settings.Port}/");
        context.SetDetail("webRoot", webRoot);
        context.Log.LogInformation("Starting DebugWeb plugin; capture is initially disabled.");
        await context.RunBackground(() => server.RunAsync(webRoot, cancellationToken));
    }
}
