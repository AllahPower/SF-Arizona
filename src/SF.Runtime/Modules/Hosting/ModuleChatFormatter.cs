namespace SFSharp.Runtime.Modules;

internal static class ModuleChatFormatter
{
    public static string Label(string value) => Paint(SFColors.Cyan | SFColors.Blue, value);
    public static string Value(string value) => Paint(SFColors.White | SFColors.Ice, value);
    public static string Paint(SFColor color, string value) => color.Apply(value);

    public static string FormatBrand() => Paint(SFColors.Yellow | SFColors.Sand, "SF");
    public static string FormatSeparator() => Paint(SFColors.Slate, " \u00b7 ");
    public static string FormatArrow() => Paint(SFColors.Slate, " \u00bb ");

    public static string FormatHeader(string title, string? badge = null)
    {
        string head = $"{FormatBrand()} {Paint(SFColors.Slate, "\u00bb")} {Paint(SFColors.White | SFColors.Ice, title)}";
        if (!string.IsNullOrWhiteSpace(badge))
        {
            head += $"  {Paint(SFColors.Slate, "[")}{Paint(SFColors.Sand, badge)}{Paint(SFColors.Slate, "]")}";
        }

        return head;
    }

    public static string FormatTip(string text) =>
        $"  {Paint(SFColors.Slate, "\u00bb")} {Paint(SFColors.White | SFColors.Ice, text)}";

    public static string FormatUsage() =>
        $"{Paint(SFColors.Yellow, "Usage")}: {Paint(SFColors.White | SFColors.Ice, "/sfs")} " +
        $"{Paint(SFColors.Slate, "[status|info|start|stop|restart|plugins|plugin-load|plugin-unload|plugin-reload|help]")}";

    public static IEnumerable<string> FormatHelpLines()
    {
        yield return FormatHeader("Command reference", "/sfs");
        yield return FormatHelpEntry("",                      "open the interactive dashboard");
        yield return FormatHelpEntry("status [page]",         "compact module list");
        yield return FormatHelpEntry("info <id>",             "detailed view with actions");
        yield return FormatHelpEntry("start|stop|restart <id>", "module lifecycle control");
        yield return FormatHelpEntry("plugins",               "list loaded plugins");
        yield return FormatHelpEntry("plugin-load <id|path>", "load plugin by id or manifest path");
        yield return FormatHelpEntry("plugin-unload <id>",    "unload plugin and its modules");
        yield return FormatHelpEntry("plugin-reload <id>",    "re-scan and reload a plugin");
        yield return FormatHelpEntry("help",                  "show this reference");
    }

    private static string FormatHelpEntry(string verb, string summary)
    {
        string left = string.IsNullOrEmpty(verb)
            ? Paint(SFColors.White | SFColors.Ice, "/sfs")
            : Paint(SFColors.White | SFColors.Ice, "/sfs ") + Paint(SFColors.Cyan | SFColors.Blue, verb);
        return $"  {left}{FormatArrow()}{Paint(SFColors.Slate, summary)}";
    }

    public static string FormatChatAction(string action, string subject, string tail, SFColor accent)
    {
        string glyph = Paint(accent, "\u00bb");
        string line = $"{glyph} {Paint(accent, action)} {Paint(SFColors.White | SFColors.Ice, subject)}";
        if (!string.IsNullOrWhiteSpace(tail))
        {
            line += $"{FormatSeparator()}{Paint(SFColors.Slate, tail)}";
        }

        return line;
    }

    public static string FormatState(ModuleLifecycleState state)
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

    public static string FormatStopReason(ModuleStopReason reason)
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

    public static string FormatBoolean(bool value)
    {
        return Paint(value ? SFColors.Green : SFColors.Red, value ? "ON" : "OFF");
    }

    public static string FormatLoad(double loadPercent)
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

    public static string FormatFaults(long faultCount)
    {
        return Paint(faultCount == 0 ? SFColors.Green : SFColors.Red, faultCount.ToString());
    }

    public static string FormatDuration(TimeSpan? value)
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

    public static string FormatBytes(long bytes)
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
