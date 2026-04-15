using Microsoft.Extensions.Logging;
using SFSharp;

namespace SFSharp.Examples.HelloWorld;

[SFModule(
    "example-hello-world",
    "Hello World Example",
    Category = "Examples",
    Description = "Minimal external module that writes Hello world! to chat and to the SF log.",
    DefaultEnabled = true,
    ExecutionModel = ModuleExecutionModel.MainThread,
    RestartPolicy = ModuleRestartPolicy.Manual)]
public sealed class HelloWorldModule : ISFModule
{
    private IModuleContext Context => ((ISFModule)this).Context;
    private ILogger Log => ((ISFModule)this).Log;

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Context.Heartbeat("hello-world");
        Context.SetStatusText("printed hello world");
        Context.Log.LogInformation(
            "Hello world from external example module pluginId={PluginId} moduleId={ModuleId}",
            "example.hello-world",
            Context.Descriptor.Id);
        Context.SF.Chat.Add("Hello world!", prefix: "[HelloWorld]", prefixColor: 0xFF55CC55);
        return Task.CompletedTask;
    }
}
