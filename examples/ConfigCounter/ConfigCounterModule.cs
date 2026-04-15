using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SFSharp;

namespace SFSharp.Examples.ConfigCounter;

[SFModule(
    "example.config-counter",
    "Config Counter Example",
    Category = "Examples",
    Description = "Persists a start counter in module config and exposes /counterexample.",
    DefaultEnabled = true,
    ExecutionModel = ModuleExecutionModel.MainThread,
    RestartPolicy = ModuleRestartPolicy.Manual)]
public sealed class ConfigCounterModule : ISFModule
{
    private CounterConfig _config = new();
    private IModuleContext Context => ((ISFModule)this).Context;
    private ILogger Log => ((ISFModule)this).Log;

    public Task OnStartingAsync()
    {
        _config = Context.Config.Load(
            ConfigCounterJsonContext.Default.CounterConfig,
            static () => new CounterConfig());

        _config.StartCount++;
        _config.LastStartedUtc = DateTimeOffset.UtcNow.ToString("O");

        Context.Config.Save(ConfigCounterJsonContext.Default.CounterConfig, _config);
        Context.RegisterChatCommand("counterexample", OnCommand);
        Context.SetDetail("config.path", Context.Config.Location);
        Context.SetDetail("user-data.root", Context.UserData.Root);
        Context.SetStatusText($"starts: {_config.StartCount}");
        Log.LogInformation(
            "Config Counter example started with StartCount={StartCount} ConfigPath={ConfigPath}",
            _config.StartCount,
            Context.Config.Location);
        Context.SF.Chat.Add(
            $"Module start count: {_config.StartCount}",
            prefix: "[ConfigCounter]",
            prefixColor: 0xFFFFAA55);
        return Task.CompletedTask;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public Task OnStoppingAsync()
    {
        Log.LogInformation("Config Counter example stopping");
        return Task.CompletedTask;
    }

    private void OnCommand(string? args)
    {
        Context.Heartbeat("command:counterexample");
        Context.IncrementCounter("commands.counterexample");
        Context.SetStatusText($"starts: {_config.StartCount}");
        Log.LogInformation("CounterExample command invoked");
        Context.SF.Chat.Add(
            $"Start count: {_config.StartCount}. Config: {Context.Config.Location}",
            prefix: "[ConfigCounter]",
            prefixColor: 0xFFFFAA55);
    }
}

public sealed class CounterConfig
{
    public int StartCount { get; set; }
    public string? LastStartedUtc { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CounterConfig))]
internal partial class ConfigCounterJsonContext : JsonSerializerContext;
