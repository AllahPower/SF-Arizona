using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SFSharp.Abstractions.Modules;
using SFSharp.Abstractions.Modules.Lifecycle;

namespace SFSharp.Examples.AntiAfkToggle;

[SFModule(
    "example.anti-afk-toggle",
    "AntiAFK Toggle",
    Category = "Examples",
    Description = "Toggles the vorbisFile.asi AntiAFK flag via /sfafk.",
    DefaultEnabled = true,
    ExecutionModel = ModuleExecutionModel.MainThread,
    RestartPolicy = ModuleRestartPolicy.Manual)]
public sealed unsafe partial class AntiAfkToggleModule : ISFModule
{
    private static readonly string[] CandidateModules = ["vorbisFile.asi", "vorbisFile.dll"];
    private const string ChatPrefix = "[AntiAFK]";
    private const uint PrefixColor = 0xFF55AAFF;
    private const uint OnColor = 0xFF66FF66;
    private const uint OffColor = 0xFFFF6666;
    private const uint WarnColor = 0xFFFFAA33;

    private IModuleContext Context => ((ISFModule)this).Context;
    private ILogger Log => ((ISFModule)this).Log;

    private delegate* unmanaged[Cdecl]<int, int> _setAntiAfk;
    private delegate* unmanaged[Cdecl]<byte> _isAntiAfk;
    private string? _resolvedModuleName;

    public Task OnStartingAsync()
    {
        if (!TryResolveExports())
        {
            Log.LogWarning("vorbisFile exports not found. /sfafk will report unavailable.");
            Context.SetStatusText("unavailable: vorbisFile not loaded");
            Context.SetDetail("state", "unavailable");
            Context.RegisterChatCommand("sfafk", OnCommandUnavailable);
            return Task.CompletedTask;
        }

        Context.RegisterChatCommand("sfafk", OnCommand);
        Context.SetDetail("export-host", _resolvedModuleName!);
        Context.SetDetail("state", ReadState() ? "on" : "off");
        Context.SetStatusText("ready, type /sfafk");
        Log.LogInformation("AntiAFK toggle attached to {Module}", _resolvedModuleName);
        Context.SF.Chat.Add(
            "Type /sfafk to toggle the AntiAFK flag. Optional: /sfafk on|off.",
            prefix: ChatPrefix,
            prefixColor: PrefixColor);
        return Task.CompletedTask;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(Timeout.Infinite, cancellationToken)
            .ContinueWith(static _ => { }, TaskContinuationOptions.OnlyOnCanceled);
    }

    public Task OnStoppingAsync()
    {
        Log.LogInformation("AntiAFK toggle stopping");
        return Task.CompletedTask;
    }

    private bool TryResolveExports()
    {
        foreach (string moduleName in CandidateModules)
        {
            nint module = GetModuleHandleW(moduleName);
            if (module == 0)
            {
                continue;
            }

            nint setAddr = GetProcAddress(module, "setAntiAfk");
            nint isAddr = GetProcAddress(module, "isAntiAfk");
            if (setAddr == 0 || isAddr == 0)
            {
                continue;
            }

            _setAntiAfk = (delegate* unmanaged[Cdecl]<int, int>)setAddr;
            _isAntiAfk = (delegate* unmanaged[Cdecl]<byte>)isAddr;
            _resolvedModuleName = moduleName;
            return true;
        }

        return false;
    }

    private bool ReadState() => _isAntiAfk() != 0;

    private void SetState(bool value) => _setAntiAfk(value ? 1 : 0);

    private void OnCommand(string? args)
    {
        bool? explicitTarget = ParseExplicitTarget(args);
        if (explicitTarget is null && !string.IsNullOrWhiteSpace(args))
        {
            Context.SF.Chat.Add(
                "Usage: /sfafk [on|off]",
                prefix: ChatPrefix,
                prefixColor: WarnColor);
            return;
        }

        bool next = explicitTarget ?? !ReadState();
        SetState(next);

        Context.IncrementCounter(next ? "afk.enabled" : "afk.disabled");
        Context.SetDetail("state", next ? "on" : "off");
        Context.Heartbeat($"afk:{(next ? "on" : "off")}");
        Log.LogInformation("AntiAFK flag set to {State}", next ? "on" : "off");

        Context.SF.Chat.Add(
            next
                ? "AntiAFK enabled. Client keeps ticking and keeps sending sync when window loses focus."
                : "AntiAFK disabled. Client throttles and stops sync while minimized.",
            prefix: ChatPrefix,
            prefixColor: next ? OnColor : OffColor);
    }

    private void OnCommandUnavailable(string? args)
    {
        Context.SF.Chat.Add(
            "AntiAFK unavailable: vorbisFile.asi / vorbisFile.dll is not loaded.",
            prefix: ChatPrefix,
            prefixColor: OffColor);
    }

    private static bool? ParseExplicitTarget(string? args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            return null;
        }

        string trimmed = args.Trim();
        if (string.Equals(trimmed, "on", StringComparison.OrdinalIgnoreCase) || trimmed == "1")
        {
            return true;
        }

        if (string.Equals(trimmed, "off", StringComparison.OrdinalIgnoreCase) || trimmed == "0")
        {
            return false;
        }

        return null;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint GetModuleHandleW(string lpModuleName);

    [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetProcAddress(nint hModule, string lpProcName);
}
