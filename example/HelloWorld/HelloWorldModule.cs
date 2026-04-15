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
    public Task RunAsync(IModuleContext context)
    {
        const string message = "Hello world!";

        context.Heartbeat("hello-world");
        context.SetStatusText("printed hello world");
        context.Log.LogInformation(
            "Hello world from external example module pluginId={PluginId} moduleId={ModuleId}",
            "example.hello-world",
            context.Descriptor.Id);
        context.SF.Chat.Add(message, prefix: "[HelloWorld]", prefixColor: 0xFF55CC55);

        return Task.CompletedTask;
    }
}
