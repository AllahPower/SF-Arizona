using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SFSharp;

public interface ISFModule
{
    Task RunAsync(CancellationToken token);
}

public class SFModuleContainer
{
    private readonly List<ISFModule> _modules = new();
    private readonly Dictionary<ISFModule, (Task Task, CancellationTokenSource TokenSource)> _runningModules = new();
    private readonly List<ISFModule> _modulesDisabledOnStart = new();

    private static SFModuleContainer? _currentContainer;

    public void RegisterModule<T>(bool enabledOnStart = true) where T : ISFModule, new()
    {
        var module = new T();
        _modules.Add(module);
        if (!enabledOnStart)
        {
            _modulesDisabledOnStart.Add(module);
        }
    }

    public void Run()
    {
        foreach (var module in _modules)
        {
            if (_modulesDisabledOnStart.Contains(module)) continue;
            var cts = new CancellationTokenSource();
            var task = module.RunAsync(cts.Token);
            _runningModules[module] = (task, cts);
        }
    }

    public async Task RunAllAsync()
    {
        foreach (var module in _modules)
        {
            if (_modulesDisabledOnStart.Contains(module)) continue;
            var cts = new CancellationTokenSource();
            var task = module.RunAsync(cts.Token);
            _runningModules[module] = (task, cts);
        }

        using var commandTaskSource = SF.Chat.RegisterChatCommand("sfs");
        await foreach(var args in commandTaskSource.StreamCommandsAsync())
        {
            await CommandCallbackCore();
        }

        await Task.WhenAll(_runningModules.Values.Select(x => x.Task));
    }

    private async Task CommandCallbackCore()
    {
        var lines = _modules.Select(x =>
        {
            var statusText = _runningModules.ContainsKey(x) ? "{00FF00}Running" : "{FF0000}Stopped";
            return $"{x.GetType().Name}\t{statusText}";
        }).ToArray();

        var dialogResult = await SF.Dialog.ShowList(
            "SFSharp modules (select to enable/disable)",
            lines,
            "Module\tStatus"
        );

        if (dialogResult is not { Button: SFDialogButton.OK })
        {
            SF.Chat.Add("[SFSharp] No changes made to running module.");
            return;
        }
        var selectedModule = _modules[dialogResult.SelectedIndex];
        if (_runningModules.TryGetValue(selectedModule, out var runningModule))
        {
            _runningModules.Remove(selectedModule);
            runningModule.TokenSource.Cancel();
            try
            {
                await runningModule.Task;
            }
            catch (OperationCanceledException) { } // Expected.

            SF.Chat.Add($"[SFSharp] {selectedModule.GetType().Name} stopped.");
        }
        else
        {
            var cts = new CancellationTokenSource();
            var task = selectedModule.RunAsync(cts.Token);
            _runningModules[selectedModule] = (task, cts);
            SF.Chat.Add($"[SFSharp] {selectedModule.GetType().Name} started.");
        }
    }
}
