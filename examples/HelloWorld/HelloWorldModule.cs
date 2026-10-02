using Microsoft.Extensions.Logging;
using SFSharp.Abstractions.Game;
using SFSharp.Abstractions.Modules;
using SFSharp.Abstractions.Modules.Lifecycle;

namespace SFSharp.Examples.HelloWorld;

[SFModule(
    "example-hello-world",
    "Hello World Example",
    Category = "Examples",
    Description = "Minimal external module that writes Hello world! and the local player's position to chat and to the SF log.",
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
        Log.LogInformation(
            "Hello world from external example module pluginId={PluginId} moduleId={ModuleId}",
            "example.hello-world",
            Context.Descriptor.Id);
        Context.SF.Chat.Add("Hello world!", prefix: "[HelloWorld]", prefixColor: 0xFF55CC55);

        ISFEntities entities = Context.SF.Entities;
        if (entities.TryGetLocalPlayer(out SFEntityRef player) && entities.TryGetSnapshot(player, out SFEntitySnapshot snapshot))
        {
            string text = $"You are at {snapshot.Position.X:0.0} {snapshot.Position.Y:0.0} {snapshot.Position.Z:0.0}, health {snapshot.Health:0}";
            Log.LogInformation("{Text}", text);
            Context.SF.Chat.Add(text, prefix: "[HelloWorld]", prefixColor: 0xFF55CC55);
        }

        return Task.CompletedTask;
    }
}
