using static SFSharp.Runtime.Modules.ModuleChatFormatter;

namespace SFSharp.Runtime.Modules;

public partial class SFModuleContainer
{
    private const int StatusPageSize = 6;

    private Action? _deferredAfterDialog;

    private async void OnCommand(string? args)
    {
        try
        {
            string[] segments = string.IsNullOrWhiteSpace(args)
                ? []
                : args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (segments.Length == 0)
            {
                await ShowDashboard();
                DispatchDeferredAfterDialog();
                return;
            }

            string verb = segments[0].ToLowerInvariant();
            if (verb is "help" or "?" or "--help")
            {
                ShowHelpInChat();
                return;
            }

            if (verb == "status")
            {
                int page = 1;
                if (segments.Length >= 2 && int.TryParse(segments[1], out int parsed))
                {
                    page = parsed;
                }

                ShowStatusInChat(page);
                return;
            }

            if (verb == "plugins")
            {
                ShowPluginsInChat();
                return;
            }

            if (verb is "plugin-load" or "plugin-unload" or "plugin-reload")
            {
                HandlePluginCommand(verb, segments);
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
                    DispatchDeferredAfterDialog();
                    break;
                case "start":
                    if (TryStartModule(registration, out string? startFailure))
                    {
                        ActivatePendingAutoStartModules();
                        SF.Chat.Add(FormatChatAction("start", registration.Descriptor.DisplayName, "requested", SFColors.Green));
                    }
                    else if (!string.IsNullOrWhiteSpace(startFailure))
                    {
                        SF.Chat.Add(FormatChatAction("start", registration.Descriptor.DisplayName, startFailure, SFColors.Red));
                        EmitStartDependencyHints(registration);
                    }
                    break;
                case "stop":
                    if (TryStopModule(registration, ModuleStopReason.UserRequested, out string? stopFailure))
                    {
                        SF.Chat.Add(FormatChatAction("stop", registration.Descriptor.DisplayName, "requested", SFColors.Orange));
                    }
                    else if (!string.IsNullOrWhiteSpace(stopFailure))
                    {
                        SF.Chat.Add(FormatChatAction("stop", registration.Descriptor.DisplayName, stopFailure, SFColors.Red));
                    }
                    break;
                case "restart":
                    if (TryRestartModule(registration, out string? restartFailure))
                    {
                        SF.Chat.Add(FormatChatAction("restart", registration.Descriptor.DisplayName, "requested", SFColors.Yellow));
                    }
                    else if (!string.IsNullOrWhiteSpace(restartFailure))
                    {
                        SF.Chat.Add(FormatChatAction("restart", registration.Descriptor.DisplayName, restartFailure, SFColors.Red));
                    }
                    break;
                default:
                    SF.Chat.Add(FormatUsage());
                    break;
            }
        }
        catch (Exception ex)
        {
            SFLog.Error(ex, "Unhandled exception in /sfs command handler");
        }
    }

    private void EmitStartDependencyHints(ModuleRegistration registration)
    {
        foreach (string depId in registration.Descriptor.Dependencies)
        {
            if (!_registrationsById.TryGetValue(depId, out ModuleRegistration? dep))
            {
                SF.Chat.Add($"  {Paint(SFColors.Slate, "dep")} {Paint(SFColors.Cyan | SFColors.Blue, depId)} {Paint(SFColors.Rose, "is not installed")}");
                continue;
            }

            if (_runningModules.ContainsKey(dep))
            {
                continue;
            }

            SF.Chat.Add($"  {Paint(SFColors.Slate, "run")} {Paint(SFColors.White | SFColors.Ice, $"/sfs start {depId}")} {Paint(SFColors.Slate, "first")}");
        }
    }

    private void ShowHelpInChat()
    {
        foreach (string line in FormatHelpLines())
        {
            SF.Chat.Add(line);
        }
    }

    private void ShowStatusInChat(int page)
    {
        if (_registrations.Count == 0)
        {
            SF.Chat.Add(FormatHeader("Modules", "0"));
            SF.Chat.Add(FormatTip("No modules registered."));
            return;
        }

        int totalPages = Math.Max(1, (_registrations.Count + StatusPageSize - 1) / StatusPageSize);
        page = Math.Clamp(page, 1, totalPages);

        List<ModuleRegistration> pageItems = _registrations
            .Skip((page - 1) * StatusPageSize)
            .Take(StatusPageSize)
            .ToList();

        int runningCount = _runningModules.Count;
        SF.Chat.Add(FormatHeader("Modules", $"{runningCount}/{_registrations.Count} running \u00b7 page {page}/{totalPages}"));

        string sep = FormatSeparator();
        foreach (ModuleRegistration registration in pageItems)
        {
            ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
            string pluginBadge = registration.OwnerPluginId is null
                ? string.Empty
                : sep + Paint(SFColors.Purple, "plugin");
            string line = string.Concat(
                "  ",
                Paint(SFColors.Cyan | SFColors.Blue, registration.Descriptor.Id),
                FormatArrow(),
                FormatState(snapshot.State),
                sep,
                Paint(SFColor.FromHex("A8DADC"), snapshot.Descriptor.ExecutionModel.ToString()),
                sep,
                Paint(SFColors.Sand, FormatDuration(snapshot.Uptime)),
                sep,
                FormatLoad(snapshot.EstimatedLoadPercent),
                pluginBadge);
            SF.Chat.Add(line);
        }

        if (totalPages > 1)
        {
            SF.Chat.Add(FormatTip($"next page: /sfs status {Math.Min(page + 1, totalPages)}"));
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
            if (_deferredAfterDialog is not null)
            {
                return;
            }
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

            if (registration.OwnerPluginId is { Length: > 0 } pluginOwner)
            {
                items.Add($"{Label("Plugin")}\t{Paint(SFColors.Purple, pluginOwner)}");
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

            int pluginReloadIndex = -1;
            int pluginUnloadIndex = -1;
            if (registration.OwnerPluginId is { Length: > 0 } && _pluginLoader is not null)
            {
                pluginReloadIndex = items.Count;
                items.Add($"{Paint(SFColors.Yellow, "[Plugin Reload]")}\t{Paint(SFColor.FromHex("FFF3D6"), "Reload owning plugin")}");
                pluginUnloadIndex = items.Count;
                items.Add($"{Paint(SFColors.Rose, "[Plugin Unload]")}\t{Paint(SFColor.FromHex("FFE0E0"), "Unload owning plugin")}");
            }

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
                    if (TryStartModule(registration, out string? startFailure))
                    {
                        ActivatePendingAutoStartModules();
                    }
                    else if (!string.IsNullOrWhiteSpace(startFailure))
                    {
                        SF.Chat.Add(FormatChatAction("start", registration.Descriptor.DisplayName, startFailure, SFColors.Red));
                    }
                    break;
                case int index when index == stopIndex:
                    if (!TryStopModule(registration, ModuleStopReason.UserRequested, out string? stopFailure) &&
                        !string.IsNullOrWhiteSpace(stopFailure))
                    {
                        SF.Chat.Add(FormatChatAction("stop", registration.Descriptor.DisplayName, stopFailure, SFColors.Red));
                    }
                    break;
                case int index when index == restartIndex:
                    if (!TryRestartModule(registration, out string? restartFailure) &&
                        !string.IsNullOrWhiteSpace(restartFailure))
                    {
                        SF.Chat.Add(FormatChatAction("restart", registration.Descriptor.DisplayName, restartFailure, SFColors.Red));
                    }
                    break;
                case int index when index == autoStartIndex:
                    registration.AutoStartEnabled = !registration.AutoStartEnabled;
                    registration.Runtime.SetAutoStartEnabled(registration.AutoStartEnabled);
                    SFHostManifest.Instance.SetEnabled(registration.Descriptor.Id, registration.AutoStartEnabled);
                    SFHostManifest.Instance.FlushSync();
                    PublishModuleCatalogSnapshot();
                    break;
                case int index when index == clearIndex:
                    registration.Runtime.ClearTelemetry();
                    break;
                case int index when pluginReloadIndex >= 0 && index == pluginReloadIndex:
                    {
                        string pluginId = registration.OwnerPluginId!;
                        DeferPluginReload(pluginId);
                        return;
                    }
                case int index when pluginUnloadIndex >= 0 && index == pluginUnloadIndex:
                    {
                        string pluginId = registration.OwnerPluginId!;
                        DeferPluginUnload(pluginId);
                        return;
                    }
            }
        }
    }

    private void ShowPluginsInChat()
    {
        if (_pluginLoader is null)
        {
            SF.Chat.Add(FormatHeader("Plugins"));
            SF.Chat.Add(FormatTip("Plugin loader is not attached."));
            return;
        }

        IReadOnlyCollection<PluginRuntimeSnapshot> plugins = _pluginLoader.LoadedPlugins;
        if (plugins.Count == 0)
        {
            SF.Chat.Add(FormatHeader("Plugins", "0"));
            SF.Chat.Add(FormatTip("No plugins loaded."));
            return;
        }

        SF.Chat.Add(FormatHeader("Plugins", plugins.Count.ToString()));
        string sep = FormatSeparator();
        foreach (PluginRuntimeSnapshot plugin in plugins.OrderBy(static plugin => plugin.PluginId, StringComparer.OrdinalIgnoreCase))
        {
            SFColor stateColor = plugin.State switch
            {
                PluginState.Loaded => SFColors.Green,
                PluginState.Unloading => SFColors.Orange,
                PluginState.UnloadFailed => SFColors.Rose,
                _ => SFColors.Slate,
            };

            string line = string.Concat(
                "  ",
                Paint(SFColors.Cyan | SFColors.Blue, plugin.PluginId),
                FormatArrow(),
                Paint(stateColor, plugin.State.ToString()),
                sep,
                Paint(SFColors.Sand, plugin.DisplayName),
                sep,
                Paint(SFColors.Slate, $"{plugin.RegisteredModuleCount} module(s)"));

            if (plugin.LastUnloadFailureReason != PluginUnloadFailureReason.None && !string.IsNullOrWhiteSpace(plugin.LastUnloadFailureMessage))
            {
                line += sep + Paint(SFColors.Rose, plugin.LastUnloadFailureReason.ToString());
            }

            SF.Chat.Add(line);
        }

        SF.Chat.Add(FormatTip("manage: /sfs plugin-reload <id>  \u00b7  /sfs plugin-unload <id>"));
    }

    private void HandlePluginCommand(string verb, string[] segments)
    {
        if (_pluginLoader is null)
        {
            SF.Chat.Add(Paint(SFColors.Rose, "Plugin loader is not attached."));
            return;
        }

        if (segments.Length < 2)
        {
            SF.Chat.Add(Paint(SFColors.Rose, "Usage: /sfs ") + Paint(SFColors.White, $"{verb} <id|manifest-path>"));
            return;
        }

        string target = string.Join(' ', segments.Skip(1)).Trim();

        switch (verb)
        {
            case "plugin-load":
                {
                    string manifestPath = target;
                    if (!File.Exists(manifestPath))
                    {
                        string candidate = Path.Combine(SFPaths.AssetsRoot, "modules", target, "module.json");
                        if (File.Exists(candidate))
                        {
                            manifestPath = candidate;
                        }
                    }

                    PluginLoadResult result = _pluginLoader.LoadFromManifest(manifestPath);
                    if (result.Success)
                    {
                        SF.Chat.Add(FormatChatAction("plugin-load", result.PluginId!, $"{result.RegisteredModuleCount} module(s)", SFColors.Green));
                    }
                    else
                    {
                        SF.Chat.Add(FormatChatAction("plugin-load", result.PluginId ?? target, result.Message, SFColors.Red));
                    }

                    break;
                }

            case "plugin-unload":
                {
                    PluginUnloadResult result = _pluginLoader.Unload(target);
                    if (result.Success)
                    {
                        SF.Chat.Add(FormatChatAction("plugin-unload", target, "done", SFColors.Orange));
                    }
                    else
                    {
                        SF.Chat.Add(FormatChatAction("plugin-unload", target, result.Message, SFColors.Red));
                    }

                    break;
                }

            case "plugin-reload":
                {
                    PluginReloadResult result = _pluginLoader.Reload(target);
                    if (result.Success)
                    {
                        SF.Chat.Add(FormatChatAction("plugin-reload", target, "done", SFColors.Yellow));
                    }
                    else
                    {
                        SF.Chat.Add(FormatChatAction("plugin-reload", target, result.Message, SFColors.Red));
                    }

                    break;
                }
        }
    }

    private void DeferPluginUnload(string pluginId)
    {
        _deferredAfterDialog = () =>
        {
            if (_pluginLoader is null)
            {
                return;
            }

            PluginUnloadResult result = _pluginLoader.Unload(pluginId);
            SFColor accent = result.Success ? SFColors.Orange : SFColors.Red;
            string tail = result.Success ? "done" : result.Message;
            SF.Chat.Add(FormatChatAction("plugin-unload", pluginId, tail, accent));
        };
    }

    private void DeferPluginReload(string pluginId)
    {
        _deferredAfterDialog = () =>
        {
            if (_pluginLoader is null)
            {
                return;
            }

            PluginReloadResult result = _pluginLoader.Reload(pluginId);
            SFColor accent = result.Success ? SFColors.Yellow : SFColors.Red;
            string tail = result.Success ? "done" : result.Message;
            SF.Chat.Add(FormatChatAction("plugin-reload", pluginId, tail, accent));
        };
    }

    private void DispatchDeferredAfterDialog()
    {
        Action? operation = _deferredAfterDialog;
        if (operation is null)
        {
            return;
        }

        _deferredAfterDialog = null;
        // Post back to the main thread so this method (and its caller OnCommand) can unwind
        // first. Executing inline would still be inside the completed-but-cached async state
        // machines of the dialog call chain, which pin plugin-owned Type references and
        // prevent the plugin's AssemblyLoadContext from being collected.
        SFBootstrap.PostToMainThread(operation);
    }

    private static string BuildDashboardLine(ModuleRegistration registration)
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
}
