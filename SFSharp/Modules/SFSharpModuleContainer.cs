using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SFSharp;

public interface ISFSharpModule
{
    Task RunAsync(CancellationToken token);
}

public class SFSharpModuleContainer
{
    private readonly List<ISFSharpModule> _modules = new();
    private readonly Dictionary<ISFSharpModule, (Task Task, CancellationTokenSource TokenSource)> _runningModules = new();
    private readonly List<ISFSharpModule> _modulesDisabledOnStart = new();

    private static SFSharpModuleContainer? _currentContainer;

    private CancellationToken _masterToken;

    public void RegisterModule<T>(bool enabledOnStart = true) where T : ISFSharpModule, new()
    {
        var module = new T();
        _modules.Add(module);
        if (!enabledOnStart)
        {
            _modulesDisabledOnStart.Add(module);
        }
    }

    public async Task RunAllAsync(CancellationToken token)
    {
        SF.RegisterChatCommand("sfs", _ => CommandCallbackCore());
        try
        {
            _masterToken = token;

            foreach (var module in _modules)
            {
                if (_modulesDisabledOnStart.Contains(module)) continue;
                var cts = CancellationTokenSource.CreateLinkedTokenSource(_masterToken);
                var task = module.RunAsync(cts.Token);
                _runningModules[module] = (task, cts);
            }

            await Task.WhenAll(_runningModules.Values.Select(x => x.Task));
        }
        finally
        {
            SF.UnregisterChatCommand("sfs");
        }
    }

    private async void CommandCallbackCore()
    {
        var lines = _modules.Select(x =>
        {
            var statusText = _runningModules.ContainsKey(x) ? "{00FF00}Running" : "{FF0000}Stopped";
            return $"{x.GetType().Name}\t{statusText}";
        }).ToArray();

        var dialogResult = await new SFDialog
        {
            Style = DialogStyle.TabListHeaders,
            Title = "SFSharp modules (select to enable/disable)",
            Header = "Module\tStatus",
            Items = lines,
            AcceptButton = "Select",
            CancelButton = "Cancel",
        }.ShowAsync();

        if (dialogResult is not { Button: DialogButton.Accept })
        {
            SF.AddChatMessage("[SFSharp] No changes made to running module.");
            return;
        }
        var selectedModule = _modules[dialogResult.SelectedItemIndex];
        if (_runningModules.TryGetValue(selectedModule, out var runningModule))
        {
            _runningModules.Remove(selectedModule);
            runningModule.TokenSource.Cancel();
            try
            {
                await runningModule.Task;
            }
            catch (OperationCanceledException) { } // Expected.

            SF.AddChatMessage($"[SFSharp] {selectedModule.GetType().Name} stopped.");
        }
        else
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_masterToken);
            var task = selectedModule.RunAsync(cts.Token);
            _runningModules[selectedModule] = (task, cts);
            SF.AddChatMessage($"[SFSharp] {selectedModule.GetType().Name} started.");
        }
    }
}
