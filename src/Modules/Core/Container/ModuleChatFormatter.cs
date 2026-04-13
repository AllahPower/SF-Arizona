namespace SFSharp;

internal static class ModuleChatFormatter
{
    public static string Label(string value) => Paint(SFColors.Cyan | SFColors.Blue, value);
    public static string Value(string value) => Paint(SFColors.White | SFColors.Ice, value);
    public static string Paint(SFColor color, string value) => color.Apply(value);

    public static string FormatUsage() =>
        $"{Paint(SFColors.Yellow, "Usage")}: {Paint(SFColors.White | SFColors.Ice, "/sfs status | /sfs info|start|stop|restart <moduleId>")}";

    public static string FormatChatAction(string action, string moduleName, string tail, SFColor accent)
    {
        return $"{Paint(accent, action)} {Paint(SFColors.White | SFColors.Ice, moduleName)} {Paint(SFColors.Slate, tail)}";
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
