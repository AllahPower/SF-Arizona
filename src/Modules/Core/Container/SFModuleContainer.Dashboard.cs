using static SFSharp.ModuleChatFormatter;

namespace SFSharp;

public partial class SFModuleContainer
{
    private const int StatusPageSize = 6;

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
                return;
            }

            string verb = segments[0].ToLowerInvariant();
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
        catch (Exception ex)
        {
            SFLog.Error(ex, "Unhandled exception in /sfs command handler");
        }
    }

    private void ShowStatusInChat(int page)
    {
        if (_registrations.Count == 0)
        {
            SF.Chat.Add(Paint(SFColors.Slate, "No modules registered."));
            return;
        }

        int totalPages = Math.Max(1, (_registrations.Count + StatusPageSize - 1) / StatusPageSize);
        page = Math.Clamp(page, 1, totalPages);

        List<ModuleRegistration> pageItems = _registrations
            .Skip((page - 1) * StatusPageSize)
            .Take(StatusPageSize)
            .ToList();

        string sep = Paint(SFColors.Slate, " \u00b7 ");
        foreach (ModuleRegistration registration in pageItems)
        {
            ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
            string line = string.Concat(
                Paint(SFColors.Cyan | SFColors.Blue, registration.Descriptor.Id),
                Paint(SFColors.White | SFColors.Ice, " \u00bb "),
                FormatState(snapshot.State),
                sep,
                Paint(SFColor.FromHex("A8DADC"), snapshot.Descriptor.ExecutionModel.ToString()),
                sep,
                Paint(SFColors.Sand, FormatDuration(snapshot.Uptime)),
                sep,
                FormatLoad(snapshot.EstimatedLoadPercent));
            SF.Chat.Add(line);
        }

        if (totalPages > 1)
        {
            SF.Chat.Add(Paint(SFColors.Slate, $"Page {page}/{totalPages}") + " " +
                         Paint(SFColors.White | SFColors.Ice, $"/sfs status <1-{totalPages}>"));
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

    private void ShowPluginsInChat()
    {
        if (_pluginLoader is null)
        {
            SF.Chat.Add(Paint(SFColors.Slate, "Plugin loader is not attached."));
            return;
        }

        IReadOnlyCollection<string> ids = _pluginLoader.LoadedPluginIds;
        if (ids.Count == 0)
        {
            SF.Chat.Add(Paint(SFColors.Slate, "No plugins loaded."));
            return;
        }

        SF.Chat.Add(Paint(SFColors.Cyan | SFColors.Blue, $"Loaded plugins ({ids.Count}):"));
        foreach (string id in ids)
        {
            SF.Chat.Add("  " + Paint(SFColors.White | SFColors.Ice, id));
        }
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

                if (_pluginLoader.TryLoadFromManifest(manifestPath, out string loadedId, out int moduleCount))
                {
                    SF.Chat.Add(FormatChatAction("plugin-load", loadedId, $"{moduleCount} module(s)", SFColors.Green));
                }
                else
                {
                    SF.Chat.Add(FormatChatAction("plugin-load", target, "failed, see sf_arz.log", SFColors.Red));
                }

                break;
            }

            case "plugin-unload":
            {
                if (_pluginLoader.TryUnload(target))
                {
                    SF.Chat.Add(FormatChatAction("plugin-unload", target, "done", SFColors.Orange));
                }
                else
                {
                    SF.Chat.Add(FormatChatAction("plugin-unload", target, "not loaded or failed", SFColors.Red));
                }

                break;
            }

            case "plugin-reload":
            {
                if (_pluginLoader.TryReload(target))
                {
                    SF.Chat.Add(FormatChatAction("plugin-reload", target, "done", SFColors.Yellow));
                }
                else
                {
                    SF.Chat.Add(FormatChatAction("plugin-reload", target, "failed, see sf_arz.log", SFColors.Red));
                }

                break;
            }
        }
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
