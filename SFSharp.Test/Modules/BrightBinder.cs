using System.Diagnostics;
using System.Text.RegularExpressions;

using SFSharp;

public class BrightBinder : ISFSharpModule
{
    private bool bbEnabled = true;
    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (bbEnabled && SF.GetAimedPlayerId() is ushort aimedPlayerId)
            {
                await ShowDialog("default", aimedPlayerId);
            }
            if (SF.IsKeyPressed(VK.XBUTTON1))
            {
                await ShowDialog("default", null);
            }
            if (SF.IsKeyPressed(VK.XBUTTON2))
            {
                bbEnabled = !bbEnabled;
                SF.AddChatMessage(bbEnabled ? "[SFSharp.Managed] Quick bind enabled." : "[SFSharp.Managed] Quick bind disabled.");
            }
            await Task.Yield();
        }
    }

    private async Task ShowDialog(string fileName, ushort? targetIdOrNull)
    {
        var currentDialog = BBDialog.FromFile(fileName);

        if(targetIdOrNull is not null && SF.GetPlayerScore(targetIdOrNull.Value) == 0)
        {
            new SFDialog
            {
                Style = DialogStyle.MsgBox,
                Title = "BrightBinder",
                Items = ["Loading player score..."],
            }.Show();
            SF.UpdateScoreAndPing();
            while(SF.GetPlayerScore(targetIdOrNull.Value) == 0)
            {
                await Task.Yield();
            }
        }

        var result = await new SFDialog
        {
            Style = DialogStyle.TabListHeaders,
            Title = $"BrightBinder: {fileName}.txt",
            Header = targetIdOrNull is ushort targetId ? $"Target: {SF.GetPlayerName(targetId)}[{targetId}] <{SF.GetPlayerScore(targetId)}>" : "No target selected.",
            Items = currentDialog.Items.Select(entry => entry.GetDisplayText()).ToArray(),
            AcceptButton = "Select",
            CancelButton = "Cancel"
        }.ShowAsync();

        if (result is not { Button: DialogButton.Accept }) return;
        var entry = currentDialog.Items[result.SelectedItemIndex];
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
        if(requiresTargetId && targetId is null)
        {
            var dialogResult = await new SFDialog
            {
                Style = DialogStyle.Input,
                Title = "BrightBinder: Input required",
                Items = ["Enter target ID:"],
                AcceptButton = "Enter",
                CancelButton = "Cancel",
            }.ShowAsync();
            if (dialogResult is not { Button: DialogButton.Accept }) return;
            ArgumentException.ThrowIfNullOrWhiteSpace(dialogResult.InputText);
            targetId = ushort.Parse(dialogResult.InputText);
        }

        var nextDialog = entry.TargetFile is null ? Task.CompletedTask : ShowDialog(entry.TargetFile, targetId);

        ushort? playerId = null;
        ushort getPlayerId() => playerId ??= SF.GetLocalPlayerId();

        string? playerName = null;
        string getPlayerName() => playerName ??= SF.GetPlayerName(getPlayerId())!;

        string? targetName = null;
        string getTargetName() => targetName ??= SF.GetPlayerName(targetId ?? getPlayerId())!;

        foreach (var rawCommand in entry.Commands)
        {
            var command = rawCommand;
            if (command.Contains(playerIdToken)) command = command.Replace(playerIdToken, getPlayerId().ToString());
            if (command.Contains(playerNameToken)) command = command.Replace(playerNameToken, getPlayerName());
            if (command.Contains(targetIdToken)) command = command.Replace(targetIdToken, targetId!.Value.ToString());
            if (command.Contains(targetNameToken)) command = command.Replace(targetNameToken, getTargetName());

            SF.SendChatMessage(command);
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
        var baseDirectory = Path.Combine(SF.SFSharpDirectory, "brightbinder");
        var filePath = Path.Combine(baseDirectory, fileName + ".txt");
        if(!File.Exists(filePath)) throw new FileNotFoundException(null, filePath);
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