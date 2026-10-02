using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp.Runtime.Diagnostics;

/// <summary>Built-in /sfd command that shows a runtime diagnostics summary in a game dialog.</summary>
internal static unsafe class RuntimeInfoCommand
{
    private const string CommandName = "sfd";
    private const string Label = "{A9C4E4}";
    private const string Value = "{FFFFFF}";
    private const string Good = "{8CE07A}";
    private const string Bad = "{FF6B6B}";
    private const string Section = "{58A6FF}";

    public static IDisposable Register(PluginLoader pluginLoader)
    {
        ArgumentNullException.ThrowIfNull(pluginLoader);

        return SF.Chat.RegisterChatCommand(CommandName, _ =>
        {
            SFLog.Debug("Debug command /sfd executed");
            SFBootstrap.ObserveTask(SF.Dialog.ShowMessage($"{RuntimeBuildInfo.ProductName} diagnostics", Build(pluginLoader)), "/sfd dialog");
        });
    }

    private static string Build(PluginLoader pluginLoader)
    {
        StringBuilder text = new();

        AppendSection(text, "Build");
        AppendLine(text, "Product", $"{RuntimeBuildInfo.ProductName} v{RuntimeBuildInfo.Version}");
        AppendLine(text, "Commit", RuntimeBuildInfo.Commit ?? "unknown");
        AppendLine(text, "Built", RuntimeBuildInfo.BuildTimestamp?.ToString("yyyy-MM-dd HH:mm 'UTC'") ?? "unknown");
        AppendLine(text, ".NET", $"{RuntimeInformation.FrameworkDescription} ({RuntimeInformation.ProcessArchitecture})");

        AppendSection(text, "Environment");
        SampVersionInfo? samp = SampEnvironment.GetOrDetect();
        AppendStatus(text, "SA-MP", samp is not null && samp.IsSupported, samp?.Version.ToString() ?? "not detected");
        SampfuncsInfo sampfuncs = samp?.Sampfuncs ?? SampfuncsInfo.NotLoaded;
        AppendStatus(text, "SAMPFUNCS", sampfuncs.IsSupported, sampfuncs.IsLoaded ? sampfuncs.VersionString ?? "unknown version" : "not loaded");
        AppendStatus(text, "_chat.asi", ModuleResolver.IsModuleLoaded("_chat.asi"), ModuleResolver.IsModuleLoaded("_chat.asi") ? "loaded" : "not loaded");
        bool azVoiceLoaded = ModuleResolver.IsModuleLoaded("AZVoice.asi");
        AppendStatus(text, "AZVoice.asi", azVoiceLoaded && IncomingAZVoicePacketHook.IsAvailable,
            !azVoiceLoaded ? "not loaded" : IncomingAZVoicePacketHook.IsAvailable ? "loaded, hooked" : "loaded, hook target not found");
        string gameState = CNetGame.TryGetInstance(out CNetGame* netGame) ? netGame->State.ToString() : "no CNetGame";
        AppendLine(text, "Network", $"game state {gameState}, server traffic {(SFBootstrap.Runtime.Hooks.IncomingRpcPacket.HasServerPlayerId ? "seen" : "not seen")}");

        AppendSection(text, "Modules");
        SFModuleInfo[] modules = [.. SF.Instance.Modules.GetAll()];
        int running = modules.Count(static module => module.State == ModuleLifecycleState.Running);
        int faulted = modules.Count(static module => module.State == ModuleLifecycleState.Faulted);
        int pluginModules = modules.Count(static module => module.IsPluginModule);
        AppendLine(text, "Registered", $"{modules.Length} ({modules.Length - pluginModules} built-in, {pluginModules} from plugins)");
        AppendStatus(text, "Running", faulted == 0, $"{running}, faulted {faulted}");
        IReadOnlyCollection<PluginRuntimeSnapshot> plugins = pluginLoader.LoadedPlugins;
        int pluginsWithWarnings = plugins.Count(static plugin => plugin.Warnings.Count != 0);
        string pluginList = string.Join(", ", plugins.Select(static plugin =>
            $"{plugin.PluginId} {plugin.Version}{(plugin.Warnings.Count == 0 ? string.Empty : " (!)")}"));
        AppendStatus(text, "Plugins", pluginsWithWarnings == 0, plugins.Count == 0 ? "none" : $"{plugins.Count}: {pluginList}");
        bool debugWebRunning = SF.Instance.Modules.TryGet("debug-web", out SFModuleInfo debugWeb) && debugWeb.State == ModuleLifecycleState.Running;
        AppendLine(text, "DebugWeb", debugWebRunning ? "http://localhost:7777/" : "stopped");

        AppendSection(text, "Process");
        using Process process = Process.GetCurrentProcess();
        AppendLine(text, "Uptime", (DateTime.Now - process.StartTime).ToString(@"hh\:mm\:ss"));
        AppendLine(text, "Memory", $"managed {GC.GetTotalMemory(false) / (1024 * 1024)} MB, working set {process.WorkingSet64 / (1024 * 1024)} MB");
        AppendLine(text, "Runtime dir", SFBootstrap.HostDirectory);
        AppendLine(text, "User data", SFPaths.UserDataRoot);
        AppendLine(text, "Log", SFLog.Path);

        return text.ToString().TrimEnd('\n');
    }

    private static void AppendSection(StringBuilder text, string title)
    {
        if (text.Length > 0)
        {
            text.Append('\n');
        }

        text.Append(Section).Append(title).Append('\n');
    }

    private static void AppendLine(StringBuilder text, string label, string value)
    {
        text.Append(Label).Append(label).Append(":\t").Append(Value).Append(value).Append('\n');
    }

    private static void AppendStatus(StringBuilder text, string label, bool ok, string value)
    {
        text.Append(Label).Append(label).Append(":\t").Append(ok ? Good : Bad).Append(value).Append('\n');
    }
}
