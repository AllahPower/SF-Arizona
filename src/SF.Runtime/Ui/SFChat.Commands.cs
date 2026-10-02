namespace SFSharp.Runtime.Ui;

public partial class SFChat : ISubHook<CInputCommandSendArgs, bool>
{
    private sealed record CommandRegistration(Dictionary<string, CommandRegistration> Owner, string Name, Action<string?> Callback) : IDisposable
    {
        public void OnCommand(string? args) => Callback(args);

        public void Dispose()
        {
            SFLog.Debug($"UnregisterChatCommand name={Name}");
            Owner.Remove(Name);
        }
    }

    private readonly Dictionary<string, CommandRegistration> _taskSourcesByCommand = new();

    public IDisposable RegisterChatCommand(string command, Action<string?> commandCallback)
    {
        SFLog.Debug($"RegisterChatCommand name={command}");
        var registration = new CommandRegistration(_taskSourcesByCommand, command, commandCallback);
        _taskSourcesByCommand.Add(command, registration);
        return registration;
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

        if (_taskSourcesByCommand.TryGetValue(commandName, out CommandRegistration? registration))
        {
            try
            {
                registration.OnCommand(commandArgs);
            }
            catch (Exception ex)
            {
                exceptions.Report(ex);
            }

            return true;
        }

        return next(args);
    }
}
