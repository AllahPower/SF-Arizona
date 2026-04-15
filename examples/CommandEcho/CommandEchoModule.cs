using Microsoft.Extensions.Logging;
using SFSharp;

namespace SFSharp.Examples.CommandEcho;

[SFModule(
    "example.command-echo",
    "Command Echo Example",
    Category = "Examples",
    Description = "Registers /echoexample and mirrors text to chat and to the SF log.",
    DefaultEnabled = true,
    ExecutionModel = ModuleExecutionModel.MainThread,
    RestartPolicy = ModuleRestartPolicy.Manual)]
public sealed class CommandEchoModule : ISFModule
{
    private IModuleContext Context => ((ISFModule)this).Context;
    private ILogger Log => ((ISFModule)this).Log;

    public Task OnStartingAsync()
    {
        Context.RegisterChatCommand("echoexample", OnCommand);
        Context.SetDetail("command", "/echoexample <text>");
        Context.SetStatusText("waiting for /echoexample");
        Log.LogInformation("Command Echo example started");
        Context.SF.Chat.Add(
            "Type /echoexample <text> to send a message through the module.",
            prefix: "[CommandEcho]",
            prefixColor: 0xFF55AAFF);
        return Task.CompletedTask;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public Task OnStoppingAsync()
    {
        Log.LogInformation("Command Echo example stopping");
        return Task.CompletedTask;
    }

    private void OnCommand(string? args)
    {
        string message = string.IsNullOrWhiteSpace(args)
            ? "Hello from Command Echo Example."
            : args.Trim();

        Context.Heartbeat("command:echoexample");
        Context.IncrementCounter("commands.echoexample");
        Context.SetStatusText($"last message: {message}");
        Log.LogInformation("EchoExample command invoked with message: {Message}", message);
        Context.SF.Chat.Add(message, prefix: "[CommandEcho]", prefixColor: 0xFF55AAFF);
    }
}
