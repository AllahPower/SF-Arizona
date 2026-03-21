using System.Diagnostics;
using System.Text.RegularExpressions;

using SFSharp;

[SFModule("brightbinder", "BrightBinder", Category = "Automation", Description = "Quick bind dialog runner with target-aware command templates.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 20)]
public class BrightBinder : SFModuleBase
{
    private bool bbEnabled = true;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Context.SetDetail("quickbind", "enabled");

        while (!cancellationToken.IsCancellationRequested)
        {
            using ModuleLoopScope _ = Context.TrackLoop("keyboard-poll");
            if (bbEnabled && SF.Players.GetAimedPlayerId() is ushort aimedPlayerId)
            {
                Context.IncrementCounter("dialogs.auto");
                await ShowDialog("default", aimedPlayerId);
            }
            if (SF.Keyboard.IsKeyPressed(VK.XBUTTON1))
            {
                Context.IncrementCounter("dialogs.manual");
                await ShowDialog("default", null);
            }
            if (SF.Keyboard.IsKeyPressed(VK.XBUTTON2))
            {
                bbEnabled = !bbEnabled;
                Context.SetDetail("quickbind", bbEnabled ? "enabled" : "disabled");
                Context.ReportActivity(bbEnabled ? "quickbind-enabled" : "quickbind-disabled");
                SF.Chat.Add(bbEnabled ? "Quick bind enabled." : "Quick bind disabled.");
            }

            Context.Heartbeat(bbEnabled ? "watching target" : "paused");
            await Task.Yield();
        }
    }

    private async Task ShowDialog(string fileName, ushort? targetIdOrNull)
    {
        Context.IncrementCounter("dialogs.opened");
        Context.SetDetail("last.dialog", fileName);
        var currentDialog = BBDialog.FromFile(fileName);

        if (targetIdOrNull is not null && SF.Players.GetScore(targetIdOrNull.Value) == 0)
        {
            _ = SF.Dialog.ShowMessage("BrightBinder", "Loading player score...");
            await SF.Players.UpdateScoreboard();
            Context.IncrementCounter("scoreboard.refreshes");
        }
        var result = await SF.Dialog.ShowList(
            $"BrightBinder: {fileName}.txt",
            currentDialog.Items.Select(entry => entry.GetDisplayText()).ToArray(),
            targetIdOrNull is ushort targetId ? $"Target: {SF.Players.GetName(targetId)}[{targetId}] <{SF.Players.GetScore(targetId)}>" : "No target selected."
        );

        if (result.Button != SFDialogButton.OK) return;
        var entry = currentDialog.Items[result.SelectedIndex];
        await ProcessEntry(entry, targetIdOrNull);
    }

    private async Task ProcessEntry(BBDialogEntry entry, ushort? targetId)
    {
        const string playerIdToken = "@playerId";
        const string playerNameToken = "@playerName";
        const string targetIdToken = "@targetId";
        const string targetNameToken = "@targetName";
        const int delay = 300;

        var requiresTargetId = entry.Commands.Any(cmd => cmd.Contains(targetIdToken) || cmd.Contains(targetNameToken));
        if (requiresTargetId && targetId is null)
        {
            var dialogResult = await SF.Dialog.ShowInput(
                "BrightBinder: Input required",
                "Enter target ID:"
            );
            if (dialogResult.Button != SFDialogButton.OK) return;
            ArgumentException.ThrowIfNullOrWhiteSpace(dialogResult.Text);
            targetId = ushort.Parse(dialogResult.Text);
        }

        var nextDialog = entry.TargetFile is null ? Task.CompletedTask : ShowDialog(entry.TargetFile, targetId);

        ushort? playerId = null;
        ushort getPlayerId() => playerId ??= SF.Players.LocalPlayerId;

        string? playerName = null;
        string getPlayerName() => playerName ??= SF.Players.GetName(getPlayerId())!;

        string? targetName = null;
        string getTargetName() => targetName ??= SF.Players.GetName(targetId ?? getPlayerId())!;

        foreach (var rawCommand in entry.Commands)
        {
            var command = rawCommand;
            if (command.Contains(playerIdToken)) command = command.Replace(playerIdToken, getPlayerId().ToString());
            if (command.Contains(playerNameToken)) command = command.Replace(playerNameToken, getPlayerName());
            if (command.Contains(targetIdToken)) command = command.Replace(targetIdToken, targetId!.Value.ToString());
            if (command.Contains(targetNameToken)) command = command.Replace(targetNameToken, getTargetName());

            SF.Chat.Send(command);
            Context.IncrementCounter("commands.sent");
            await Task.Delay(delay);
        }

        await nextDialog;
    }
}

public record BBDialogEntry(string[] Commands, string? DisplayText, string? TargetFile)
{
    public string GetDisplayText() => DisplayText ?? string.Join("; ", Commands);

    public static BBDialogEntry FromLine(string dialogName, string line)
    {
        var match = RegexHelper.DialogEntry().Match(line);
        if (!match.Success) throw new UnreachableException("Could not parse dialog entry!");
        return new(
            Commands: match.Groups["commands"].Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            DisplayText: match.Groups["displayText"].Success ? match.Groups["displayText"].Value.Trim() : null,
            TargetFile: match.Groups["targetFile"].Success ? match.Groups["targetFile"].Value.Trim() : match.Groups["loopback"].Success ? dialogName : null
        );
    }
}

public record BBDialog(string Name, BBDialogEntry[] Items)
{
    public static BBDialog FromFile(string fileName)
    {
        var baseDirectory = Path.Combine(SF.UserFilesDirectory, "SF", "brightbinder");
        var filePath = Path.Combine(baseDirectory, fileName + ".txt");
        if (!File.Exists(filePath)) throw new FileNotFoundException(null, filePath);
        return new(
            Name: fileName,
            Items: File.ReadAllLines(filePath)
                .Select(line => line.Trim())
                //.Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => BBDialogEntry.FromLine(fileName, line))
                .ToArray()
        );
    }
}

public static partial class RegexHelper
{
    /* Command1;Command2;Command3 => nextFileName // DisplayText - run commands, proceed to menu "nextFileName" */
    /* Command1;Command2;Command3 <= // DisplayText - run commands, reopen the same menu */
    [GeneratedRegex(@"\A(?<commands>.*?)\s*(?:=>\s*(?<targetFile>\w+)|(?<loopback><=))?\s*(?://(?<displayText>.+))?\z")]
    public static partial Regex DialogEntry();
}
