using SFSharp;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public interface ISFModule
{
    Task RunAsync(CancellationToken token);
}

public class SFModuleContainer
{
    private readonly List<ISFModule> _moduleRepository = new();
    private readonly Dictionary<ISFModule, (Task Task, CancellationTokenSource TokenSource)> _runningModules = new();
    private readonly List<ISFModule> _modulesDisabledOnStart = new();

    private TaskCompletionSource _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void RegisterModule<T>(bool enabledOnStart = true) where T : ISFModule, new()
    {
        var module = new T();
        _moduleRepository.Add(module);
        SFLog.Info($"RegisterModule module={module.GetType().Name} enabledOnStart={enabledOnStart}");
        if (!enabledOnStart)
        {
            _modulesDisabledOnStart.Add(module);
        }
    }

    private void StartModule(ISFModule module)
    {
        if (GetModuleStatus(module) != ModuleStatus.Stopped)
        {
            throw new UnreachableException();
        }

        SFLog.Info($"StartModule module={module.GetType().Name}");
        var cts = new CancellationTokenSource();
        var task = module.RunAsync(cts.Token);
        _runningModules.Add(module, (task, cts));

        _moduleStartTaskSource.SetResult();
        _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private void StopModule(ISFModule module)
    {
        if (GetModuleStatus(module) != ModuleStatus.Running)
        {
            throw new UnreachableException();
        }

        SFLog.Info($"StopModule module={module.GetType().Name}");
        _runningModules[module].TokenSource.Cancel();
    }

    public async Task Run(CancellationToken token = default)
    {
        SFLog.Info("SFModuleContainer.Run started");
        foreach (var module in _moduleRepository)
        {
            if (_modulesDisabledOnStart.Contains(module))
            {
                continue;
            }

            StartModule(module);
        }

        using var commandRegistration = SF.Chat.RegisterChatCommand("sfs", OnCommand);

        while (!token.IsCancellationRequested)
        {
            var moduleStartTask = _moduleStartTaskSource.Task;
            var tasks = _runningModules.Values.Select(x => x.Task).Append(moduleStartTask).ToArray();
            var completedTask = await Task.WhenAny(tasks);

            if (completedTask == moduleStartTask)
            {
                _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
                SFLog.Info("Module start signal observed");
                continue;
            }

            var completedModule = _runningModules.SingleOrDefault(x => x.Value.Task == completedTask).Key;
            _runningModules.Remove(completedModule);

            switch (completedTask.Status)
            {
                case TaskStatus.RanToCompletion:
                case TaskStatus.Canceled:
                    SFLog.Info($"Module completed module={completedModule.GetType().Name} status={completedTask.Status}");
                    SF.Chat.Add($"{completedModule.GetType().Name} completed.");
                    break;
                case TaskStatus.Faulted:
                    SFLog.Error(completedTask.Exception!.GetBaseException(), $"Module faulted module={completedModule.GetType().Name}");
                    SF.Chat.Add($"{completedModule.GetType().Name} threw an exception:");
                    var exception = completedTask.Exception!.GetBaseException();
                    SF.Chat.Add($"{exception.GetType().Name}: {exception.Message}");
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        SFLog.Warn("SFModuleContainer.Run cancelled");
    }

    private enum ModuleStatus
    {
        Running,
        Stopped,
        WaitingToExit
    }

    private ModuleStatus GetModuleStatus(ISFModule module)
    {
        if (_runningModules.TryGetValue(module, out var moduleInfo))
        {
            return moduleInfo.TokenSource.IsCancellationRequested ? ModuleStatus.WaitingToExit : ModuleStatus.Running;
        }

        return ModuleStatus.Stopped;
    }

    private string GetModuleStatusText(ISFModule module)
    {
        return GetModuleStatus(module) switch
        {
            ModuleStatus.Running => "{00FF00}Running",
            ModuleStatus.WaitingToExit => "{A0A0A0}Waiting to exit",
            ModuleStatus.Stopped => "{FF0000}Stopped",
            _ => throw new UnreachableException()
        };
    }

    private async void OnCommand(string? args)
    {
        SFLog.Info($"/sfs command invoked args={args ?? "<null>"}");
        var dialogResult = await SF.Dialog.ShowList(
            "SF modules (select to enable/disable)",
            _moduleRepository.Select(x => $"{x.GetType().Name}\t{GetModuleStatusText(x)}"),
            "Module\tStatus"
        );
        if (dialogResult.Button != SFDialogButton.OK)
        {
            SFLog.Info("/sfs cancelled by user");
            SF.Chat.Add("No changes made to running modules.");
            return;
        }

        var selectedModule = _moduleRepository[dialogResult.SelectedIndex];
        SFLog.Info($"/sfs selected module={selectedModule.GetType().Name} status={GetModuleStatus(selectedModule)}");
        switch (GetModuleStatus(selectedModule))
        {
            case ModuleStatus.WaitingToExit:
                SF.Chat.Add($"{selectedModule.GetType().Name} is already waiting to exit.");
                break;
            case ModuleStatus.Running:
                StopModule(selectedModule);
                break;
            case ModuleStatus.Stopped:
                SF.Chat.Add($"Starting {selectedModule.GetType().Name}.");
                StartModule(selectedModule);
                break;
        }
    }
}
