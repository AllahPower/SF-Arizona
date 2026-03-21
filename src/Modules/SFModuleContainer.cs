using System.Diagnostics;

namespace SFSharp;

public class SFModuleContainer
{
    private sealed class ModuleRegistration(ModuleDescriptor descriptor, Func<ISFModule> factory, bool autoStartEnabled)
    {
        public ModuleDescriptor Descriptor { get; } = descriptor;
        public Func<ISFModule> Factory { get; } = factory;
        public ModuleRuntimeInfo Runtime { get; } = new(descriptor, autoStartEnabled);
        public bool AutoStartEnabled { get; set; } = autoStartEnabled;
    }

    private sealed record RunningModule(ISFModule Instance, ModuleContext Context, Task Task, CancellationTokenSource CancellationTokenSource);

    private readonly List<ModuleRegistration> _registrations = [];
    private readonly Dictionary<string, ModuleRegistration> _registrationsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ModuleRegistration, RunningModule> _runningModules = [];
    private TaskCompletionSource _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _isShuttingDown;

    public void RegisterModule<T>(bool? enabledOnStart = null) where T : ISFModule, new()
    {
        ModuleDescriptor descriptor = ModuleDescriptor.FromType(typeof(T));
        if (_registrationsById.ContainsKey(descriptor.Id))
        {
            throw new InvalidOperationException($"Module id '{descriptor.Id}' is already registered.");
        }

        bool autoStartEnabled = enabledOnStart ?? descriptor.DefaultEnabled;
        ModuleRegistration registration = new(descriptor, static () => new T(), autoStartEnabled);
        _registrations.Add(registration);
        _registrations.Sort((left, right) =>
        {
            int order = left.Descriptor.Order.CompareTo(right.Descriptor.Order);
            return order != 0 ? order : string.Compare(left.Descriptor.DisplayName, right.Descriptor.DisplayName, StringComparison.OrdinalIgnoreCase);
        });
        _registrationsById.Add(descriptor.Id, registration);
        SFLog.Info($"RegisterModule id={descriptor.Id} type={descriptor.ModuleType.Name} enabledOnStart={autoStartEnabled} execution={descriptor.ExecutionModel}");
    }

    public async Task Run(CancellationToken token = default)
    {
        SFLog.Info("SFModuleContainer.Run started");
        foreach (ModuleRegistration registration in _registrations)
        {
            if (!registration.AutoStartEnabled)
            {
                continue;
            }

            StartModule(registration);
        }

        using IDisposable commandRegistration = SF.Chat.RegisterChatCommand("sfs", OnCommand);

        try
        {
            while (!token.IsCancellationRequested)
            {
                Task moduleStartTask = _moduleStartTaskSource.Task;
                Task[] tasks = _runningModules.Values.Select(x => x.Task).Append(moduleStartTask).ToArray();
                Task completedTask = await Task.WhenAny(tasks);

                if (completedTask == moduleStartTask)
                {
                    _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    continue;
                }

                KeyValuePair<ModuleRegistration, RunningModule> completed = _runningModules.Single(x => x.Value.Task == completedTask);
                _runningModules.Remove(completed.Key);
                await HandleModuleCompletion(completed.Key, completed.Value);
            }
        }
        finally
        {
            _isShuttingDown = true;
            foreach ((ModuleRegistration registration, RunningModule running) in _runningModules.ToArray())
            {
                StopModule(registration, ModuleStopReason.ContainerShutdown);
            }

            if (_runningModules.Count > 0)
            {
                await Task.WhenAll(_runningModules.Values.Select(x => x.Task));
            }
        }
    }

    private void StartModule(ModuleRegistration registration)
    {
        if (_runningModules.ContainsKey(registration))
        {
            return;
        }

        registration.Runtime.SetAutoStartEnabled(registration.AutoStartEnabled);
        registration.Runtime.PrepareForStart(registration.AutoStartEnabled);
        ISFModule module = registration.Factory();
        CancellationTokenSource cts = new();
        ModuleContext context = new(registration.Descriptor, registration.Runtime, cts.Token);

        SFLog.Info($"StartModule id={registration.Descriptor.Id} type={registration.Descriptor.ModuleType.Name} execution={registration.Descriptor.ExecutionModel}");
        Task task = registration.Descriptor.ExecutionModel switch
        {
            ModuleExecutionModel.BackgroundWorker => Task.Run(() => RunModuleCore(registration, module, context), cts.Token),
            _ => RunModuleCore(registration, module, context)
        };

        _runningModules.Add(registration, new(module, context, task, cts));
        _moduleStartTaskSource.TrySetResult();
        _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private async Task RunModuleCore(ModuleRegistration registration, ISFModule module, ModuleContext context)
    {
        registration.Runtime.MarkRunning();
        try
        {
            await module.RunAsync(context);
            registration.Runtime.MarkCompleted();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            registration.Runtime.MarkCompleted();
        }
        catch (Exception ex)
        {
            registration.Runtime.MarkFault(ex);
            throw;
        }
        finally
        {
            context.Dispose();
        }
    }

    private void StopModule(ModuleRegistration registration, ModuleStopReason reason)
    {
        if (!_runningModules.TryGetValue(registration, out RunningModule? running))
        {
            return;
        }

        SFLog.Info($"StopModule id={registration.Descriptor.Id} reason={reason}");
        registration.Runtime.RequestStop(reason);
        running.CancellationTokenSource.Cancel();
    }

    private async Task HandleModuleCompletion(ModuleRegistration registration, RunningModule running)
    {
        try
        {
            await running.Task;
            ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
            SFLog.Info($"Module completed id={registration.Descriptor.Id} state={snapshot.State} stopReason={snapshot.LastStopReason}");
            if (snapshot.LastStopReason == ModuleStopReason.Faulted)
            {
                SF.Chat.Add(FormatChatAction("faulted", registration.Descriptor.DisplayName, "faulted", SFColors.Red));
            }
            else
            {
                SF.Chat.Add(FormatChatAction("stopped", registration.Descriptor.DisplayName, snapshot.LastStopReason.ToString(), SFColors.Orange));
            }
        }
        catch (Exception ex)
        {
            SFLog.Error(ex.GetBaseException(), $"Module faulted id={registration.Descriptor.Id}");
            Exception baseException = ex.GetBaseException();
            SF.Chat.Add(FormatChatAction("faulted", registration.Descriptor.DisplayName, "exception", SFColors.Red));
            SF.Chat.Add((SFColors.Rose | SFColors.White).Apply($"{baseException.GetType().Name}: {baseException.Message}"));
        }
        finally
        {
            running.CancellationTokenSource.Dispose();
        }

        ModuleRuntimeSnapshot finalSnapshot = registration.Runtime.CreateSnapshot();
        if (!_isShuttingDown && finalSnapshot.LastStopReason == ModuleStopReason.RestartRequested)
        {
            registration.Runtime.IncrementRestartCount();
            StartModule(registration);
            return;
        }

        if (!_isShuttingDown && finalSnapshot.LastStopReason == ModuleStopReason.Faulted && registration.Descriptor.RestartPolicy == ModuleRestartPolicy.OnFault)
        {
            registration.Runtime.IncrementRestartCount();
            StartModule(registration);
        }
    }

    private async void OnCommand(string? args)
    {
        string[] segments = string.IsNullOrWhiteSpace(args)
            ? []
            : args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
        {
            await ShowDashboard();
            return;
        }

        string verb = segments[0].ToLowerInvariant();
        if (verb == "status")
        {
            ShowStatusInChat();
            return;
        }

        if (segments.Length < 2)
        {
            SF.Chat.Add(FormatUsage());
            return;
        }

        string query = string.Join(' ', segments.Skip(1));
        ModuleRegistration? registration = ResolveModule(query);
        if (registration is null)
        {
            SF.Chat.Add($"{Paint(SFColors.Rose, "Module not found")}: {Paint(SFColors.White | SFColors.Ice, query)}");
            return;
        }

        switch (verb)
        {
            case "info":
                await ShowModuleDetail(registration);
                break;
            case "start":
                StartModule(registration);
                SF.Chat.Add(FormatChatAction("start", registration.Descriptor.DisplayName, "requested", SFColors.Green));
                break;
            case "stop":
                StopModule(registration, ModuleStopReason.UserRequested);
                SF.Chat.Add(FormatChatAction("stop", registration.Descriptor.DisplayName, "requested", SFColors.Orange));
                break;
            case "restart":
                RestartModule(registration);
                SF.Chat.Add(FormatChatAction("restart", registration.Descriptor.DisplayName, "requested", SFColors.Yellow));
                break;
            default:
                SF.Chat.Add(FormatUsage());
                break;
        }
    }

    private void RestartModule(ModuleRegistration registration)
    {
        if (_runningModules.ContainsKey(registration))
        {
            StopModule(registration, ModuleStopReason.RestartRequested);
            return;
        }

        registration.Runtime.IncrementRestartCount();
        StartModule(registration);
    }

    private void ShowStatusInChat()
    {
        foreach (ModuleRegistration registration in _registrations)
        {
            ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
            string uptime = FormatDuration(snapshot.Uptime);
            string line = string.Join(" ", [
                Paint(SFColors.Cyan | SFColors.Blue, registration.Descriptor.Id),
                Paint(SFColors.Slate, "|"),
                FormatState(snapshot.State),
                Paint(SFColors.Slate, "|"),
                Paint(SFColor.FromHex("A8DADC"), snapshot.Descriptor.ExecutionModel.ToString()),
                Paint(SFColors.Slate, "|"),
                Paint(SFColors.Sand, $"uptime={uptime}"),
                Paint(SFColors.Slate, "|"),
                FormatLoad(snapshot.EstimatedLoadPercent)
            ]);
            SF.Chat.Add(line);
        }
    }

    private async Task ShowDashboard()
    {
        while (true)
        {
            string[] items = _registrations.Select(BuildDashboardLine).ToArray();
            SFColor titleColor = SFColors.Yellow | SFColors.Sand;
            SFColor headerColor = SFColors.Cyan | SFColors.Blue;
            var result = await SF.Dialog.ShowList(
                titleColor.Apply("SF Modules"),
                items,
                $"{headerColor.Apply("Module")}\t{headerColor.Apply("State")}\t{headerColor.Apply("Exec")}\t{headerColor.Apply("Uptime")}\t{headerColor.Apply("Load")}\t{headerColor.Apply("Last activity")}");
            if (result.Button != SFDialogButton.OK)
            {
                return;
            }

            ModuleRegistration selected = _registrations[result.SelectedIndex];
            await ShowModuleDetail(selected);
        }
    }

    private async Task ShowModuleDetail(ModuleRegistration registration)
    {
        while (true)
        {
            ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
            string stateText = FormatState(snapshot.State);
            string autoStartText = FormatBoolean(snapshot.AutoStartEnabled);
            string loadText = FormatLoad(snapshot.EstimatedLoadPercent);
            string faultText = FormatFaults(snapshot.FaultCount);
            string stopReasonText = FormatStopReason(snapshot.LastStopReason);
            List<string> items =
            [
                $"{Label("Id")}\t{Value(registration.Descriptor.Id)}",
                $"{Label("State")}\t{stateText}",
                $"{Label("Exec")}\t{Value(registration.Descriptor.ExecutionModel.ToString())}",
                $"{Label("Autostart")}\t{autoStartText}",
                $"{Label("Uptime")}\t{Value(FormatDuration(snapshot.Uptime))}",
                $"{Label("Load")}\t{loadText}",
                $"{Label("Threads")}\t{Value($"start={snapshot.StartThreadId?.ToString() ?? "-"} last={snapshot.LastThreadId?.ToString() ?? "-"}")}",
                $"{Label("Loops")}\t{Value($"count={snapshot.LoopCount} avg={snapshot.AverageLoopMilliseconds:0.00}ms max={snapshot.MaxLoopMilliseconds:0.00}ms")}",
                $"{Label("Activity")}\t{Value(snapshot.LastActivity ?? "-")}",
                $"{Label("Memory")}\t{Value(FormatBytes(snapshot.LastMemoryBytes))}",
                $"{Label("Restarts")}\t{Value(snapshot.RestartCount.ToString())}",
                $"{Label("Faults")}\t{faultText}",
                $"{Label("Last stop")}\t{stopReasonText}"
            ];

            if (!string.IsNullOrWhiteSpace(registration.Descriptor.Description))
            {
                items.Add($"{Label("Description")}\t{Value(registration.Descriptor.Description)}");
            }

            if (!string.IsNullOrWhiteSpace(snapshot.LastExceptionType))
            {
                items.Add($"{Label("Last error")}\t{Paint(SFColors.Rose, $"{snapshot.LastExceptionType}: {snapshot.LastExceptionMessage}")}");
            }

            foreach ((string key, long value) in snapshot.Counters.OrderBy(x => x.Key).Take(8))
            {
                items.Add($"{Paint(SFColor.FromHex("5BC0EB"), $"Counter:{key}")}\t{Paint(SFColors.Ice, value.ToString())}");
            }

            foreach ((string key, string value) in snapshot.Details.OrderBy(x => x.Key).Take(8))
            {
                items.Add($"{Paint(SFColor.FromHex("9B5DE5"), $"Detail:{key}")}\t{Paint(SFColors.White | SFColors.Ice, value)}");
            }

            int startIndex = items.Count;
            items.Add($"{Paint(SFColors.Green, "[Start]")}\t{Paint(SFColor.FromHex("E8F5E9"), "Launch module")}");
            int stopIndex = items.Count;
            items.Add($"{Paint(SFColors.Red, "[Stop]")}\t{Paint(SFColor.FromHex("FFEBEE"), "Stop module")}");
            int restartIndex = items.Count;
            items.Add($"{Paint(SFColors.Orange, "[Restart]")}\t{Paint(SFColor.FromHex("FFF8E1"), "Restart module")}");
            int autoStartIndex = items.Count;
            items.Add(snapshot.AutoStartEnabled
                ? $"{Paint(SFColors.Purple, "[Autostart]")}\t{Paint(SFColor.FromHex("F3E5F5"), "Disable autostart")}"
                : $"{Paint(SFColors.Mint, "[Autostart]")}\t{Paint(SFColor.FromHex("E0F2F1"), "Enable autostart")}");
            int clearIndex = items.Count;
            items.Add($"{Paint(SFColors.Blue, "[Telemetry]")}\t{Paint(SFColor.FromHex("E1F5FE"), "Clear counters and details")}");

            SFColor detailTitleColor = SFColors.Yellow | SFColors.Orange;
            SFColor detailHeaderColor = SFColors.Cyan | SFColors.Ice;
            var result = await SF.Dialog.ShowList(
                detailTitleColor.Apply(registration.Descriptor.DisplayName),
                items,
                $"{detailHeaderColor.Apply("Field")}\t{detailHeaderColor.Apply("Value")}");
            if (result.Button != SFDialogButton.OK)
            {
                return;
            }

            switch (result.SelectedIndex)
            {
                case int index when index == startIndex:
                    StartModule(registration);
                    break;
                case int index when index == stopIndex:
                    StopModule(registration, ModuleStopReason.UserRequested);
                    break;
                case int index when index == restartIndex:
                    RestartModule(registration);
                    break;
                case int index when index == autoStartIndex:
                    registration.AutoStartEnabled = !registration.AutoStartEnabled;
                    registration.Runtime.SetAutoStartEnabled(registration.AutoStartEnabled);
                    break;
                case int index when index == clearIndex:
                    registration.Runtime.ClearTelemetry();
                    break;
            }
        }
    }

    private string BuildDashboardLine(ModuleRegistration registration)
    {
        ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
        return string.Join('\t', [
            Paint(SFColors.White | SFColors.Ice, registration.Descriptor.DisplayName),
            FormatState(snapshot.State),
            Paint(SFColor.FromHex("A8DADC"), registration.Descriptor.ExecutionModel.ToString()),
            Paint(SFColors.Sand, FormatDuration(snapshot.Uptime)),
            FormatLoad(snapshot.EstimatedLoadPercent),
            Paint(SFColor.FromHex("B8C0C2"), snapshot.LastActivity ?? "-")
        ]);
    }

    private ModuleRegistration? ResolveModule(string query)
    {
        if (_registrationsById.TryGetValue(query, out ModuleRegistration? exact))
        {
            return exact;
        }

        string normalized = query.Trim();
        return _registrations.FirstOrDefault(x =>
            string.Equals(x.Descriptor.DisplayName, normalized, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Descriptor.ModuleType.Name, normalized, StringComparison.OrdinalIgnoreCase) ||
            x.Descriptor.Id.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
            x.Descriptor.DisplayName.Contains(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string Label(string value) => Paint(SFColors.Cyan | SFColors.Blue, value);
    private static string Value(string value) => Paint(SFColors.White | SFColors.Ice, value);
    private static string Paint(SFColor color, string value) => color.Apply(value);
    private static string FormatUsage() => $"{Paint(SFColors.Yellow, "Usage")}: {Paint(SFColors.White | SFColors.Ice, "/sfs status | /sfs info|start|stop|restart <moduleId>")}";

    private static string FormatChatAction(string action, string moduleName, string tail, SFColor accent)
    {
        return $"{Paint(accent, action)} {Paint(SFColors.White | SFColors.Ice, moduleName)} {Paint(SFColors.Slate, tail)}";
    }

    private static string FormatState(ModuleLifecycleState state)
    {
        SFColor color = state switch
        {
            ModuleLifecycleState.Created => SFColor.FromHex("90A4AE"),
            ModuleLifecycleState.Starting => SFColors.Blue,
            ModuleLifecycleState.Running => SFColors.Green,
            ModuleLifecycleState.Stopping => SFColors.Orange,
            ModuleLifecycleState.Stopped => SFColors.Slate,
            ModuleLifecycleState.Faulted => SFColors.Red,
            _ => SFColors.White
        };
        return Paint(color, state.ToString());
    }

    private static string FormatStopReason(ModuleStopReason reason)
    {
        SFColor color = reason switch
        {
            ModuleStopReason.Completed => SFColors.Green,
            ModuleStopReason.UserRequested => SFColors.Blue,
            ModuleStopReason.RestartRequested => SFColors.Orange,
            ModuleStopReason.ContainerShutdown => SFColors.Slate,
            ModuleStopReason.Faulted => SFColors.Red,
            _ => SFColors.White | SFColors.Ice
        };
        return Paint(color, reason.ToString());
    }

    private static string FormatBoolean(bool value)
    {
        return Paint(value ? SFColors.Green : SFColors.Red, value ? "ON" : "OFF");
    }

    private static string FormatLoad(double loadPercent)
    {
        SFColor color = loadPercent switch
        {
            < 10 => SFColors.Slate,
            < 35 => SFColors.Green,
            < 70 => SFColors.Orange,
            _ => SFColors.Red
        };
        return Paint(color, $"{loadPercent:0.0}%");
    }

    private static string FormatFaults(long faultCount)
    {
        return Paint(faultCount == 0 ? SFColors.Green : SFColors.Red, faultCount.ToString());
    }
    private static string FormatDuration(TimeSpan? value)
    {
        if (value is null)
        {
            return "-";
        }

        TimeSpan duration = value.Value;
        if (duration.TotalSeconds < 1)
        {
            return "<1s";
        }

        if (duration.TotalMinutes < 1)
        {
            return $"{(int)duration.TotalSeconds}s";
        }

        if (duration.TotalHours < 1)
        {
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        }

        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double value = bytes;
        int unitIndex = 0;
        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value:0.0}{units[unitIndex]}";
    }
}
