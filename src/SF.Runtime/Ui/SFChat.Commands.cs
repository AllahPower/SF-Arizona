namespace SFSharp.Runtime.Ui;

public partial class SFChat : ISubHook<CInputCommandSendArgs, bool>
{
    private readonly ChatCommandRegistry _commands = new();

    public IDisposable RegisterChatCommand(string command, Action<string?> commandCallback)
    {
        SFLog.Debug($"RegisterChatCommand name={command}");
        return _commands.Register(command, commandCallback);
    }

    bool ISubHook<CInputCommandSendArgs, bool>.Process(CInputCommandSendArgs args, Func<CInputCommandSendArgs, bool> next)
    {
        if (!args.Text.StartsWith('/'))
        {
            return next(args);
        }

        string commandLine = args.Text[1..];
        int separatorIndex = commandLine.IndexOf(' ');
        string commandName = separatorIndex >= 0 ? commandLine[..separatorIndex] : commandLine;
        string? commandArgs = separatorIndex >= 0 ? commandLine[(separatorIndex + 1)..] : null;

        try
        {
            if (_commands.TryInvoke(commandName, commandArgs)) return true;
        }
        catch (Exception ex)
        {
            exceptions.Report(ex);
            return true;
        }

        return next(args);
    }
}
